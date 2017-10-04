using Delaunay.Geo;
using Delaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGenerator : MonoBehaviour
{

    [SerializeField]
    private string seed = "lelele";
    [SerializeField]
    private bool randomSeed = false;
    [SerializeField]
    private int pointCount = 150;
    [SerializeField]
    private int width = 1000;
    [SerializeField]
    private int height = 1000;
    [SerializeField, Range(0f, 0.25f)]
    private float borderPercentage = 0.15f;
    [SerializeField]
    private int relaxation = 0;
    [SerializeField]
    private bool drawVoronoi = true, drawDelaunay = false, drawSpanningTree = false;

    private System.Random random;
    private Voronoi voronoi;
    private List<Vector2> roomOriginSites;
    private List<Vector2> pathSites;
    private List<List<Vector2>> _layout;
    private float maxCoordX = 0;
    private float minCoordX = 0;
    private float maxCoordY = 0;
    private float minCoordY = 0;
    private List<Vector2> outOfBoundsCoordinates;

    /// <summary>
    /// Return the same random every time this is called
    /// </summary>
    /// <returns></returns>
    private System.Random GetRandom()
    {
        if (random == null)
            random = new System.Random(randomSeed.GetHashCode());
        return random;
    }

    /// <summary>
    /// Returns true when the coordinate is too close to the voronoi borders
    /// </summary>
    /// <param name="coord"></param>
    /// <returns></returns>
    private bool IsOutOfBounds(Vector2 coord)
    {
        bool outOfBounds = true;

        if (coord.x < minCoordX || coord.x > maxCoordX || coord.y < minCoordY || coord.y > maxCoordY)
            outOfBoundsCoordinates.Add(coord);
        else
            outOfBounds = false;

        return outOfBounds;
    }

    /// <summary>
    /// Generate the level layout based on a voronoi diagram and level parameters.
    /// NOTE: Layout currently consists of only rooms
    /// </summary>
    /// <returns></returns>
    public List<List<Vector2>> GenerateLayout(int roomSize, int roomCount)
    {
        // Init vars
        voronoi = LayoutUtil.GenerateVoronoiObject(pointCount,width,height,GetRandom());
        roomOriginSites = new List<Vector2>();
        pathSites = new List<Vector2>();
        maxCoordX = width - (width * borderPercentage * 0.5f);
        maxCoordY = height - (height * borderPercentage * 0.5f);
        minCoordX = width * borderPercentage * 0.5f;
        minCoordY = height * borderPercentage * 0.5f;
        outOfBoundsCoordinates = new List<Vector2>();
        List<List<Vector2>> layout = new List<List<Vector2>>();

        // Randomize seed when needed
        if (randomSeed)
            seed = System.DateTime.Now.ToString();

        //Apply Lloyd's relaxation to voronoi 
        for (int i = 0; i < relaxation; i++)
        {
            voronoi = LayoutUtil.RelaxVoronoi(voronoi);
        }

        //Generate rooms and add them to the layout
        for (int i = 0; i < roomCount; i++)
        {
            Vector2 roomOriginSite;
            List<List<Vector2>> room = GenerateRoom(voronoi, roomSize, seed + i, out roomOriginSite);
            if (room == null)
            {
                Debug.LogWarning("Failed to generate room " + i);
                return null;
            }
            layout.AddRange(room);
            roomOriginSites.Add(roomOriginSite);
        }

        //Generate paths between rooms and add them to the layout
        //TEMP: simply generate a path between 2 following rooms
        //TODO: remove duplicate vertices <===========================<==================<==================<============<!!!!@$@#$^#$%@^!@#$%
        for (int i = 0; i < roomOriginSites.Count - 1; i++)
        {
            // if there are no more rooms in line, loop back to the first room.
            //Vector2 target = i + 1 < roomOriginSites.Count ? roomOriginSites[i + 1] : roomOriginSites[i];
            Vector2 start = roomOriginSites[i];
            Vector2 target = roomOriginSites[i + 1];
            List<Vector2> pathSites = LayoutUtil.GetSitePathToTarget(voronoi, start, target);
            this.pathSites.AddRange(pathSites);

            List<List<Vector2>> path = LayoutUtil.GetVerticesForSites(voronoi, pathSites);
            layout.AddRange(path);
        }

        Debug.Log("Pieces in layout: " + layout.Count);
        _layout = layout;
        return layout;
    }

    

    /// <summary>
    /// Generate a room by selecting a random cell from a given voronoi grid and adding the surrounding cells
    /// to it until the required size has been met.
    /// TODO: Incorporate 'GetVerticesForSite' for cleaner code.
    /// </summary>
    /// <param name="voronoi"></param>
    /// <returns></returns>
    public List<List<Vector2>> GenerateRoom(Voronoi voronoi, int size, string seed, out Vector2 roomOriginSite)
    {
        if (size < 1)
        {
            roomOriginSite = Vector2.zero;
            return null;
        }

        //Select a random site from the voronoi diagram within the set borders
        roomOriginSite = RandomUtil.RandomElement(voronoi.SiteCoords(), false, seed);
        while (IsOutOfBounds(roomOriginSite))
        {
            roomOriginSite = RandomUtil.RandomElement(voronoi.SiteCoords(), false, seed + outOfBoundsCoordinates.Count);
        }

        //Start building the final room from the cell at the coord
        List<LineSegment> baseRoom = voronoi.VoronoiBoundaryForSite(roomOriginSite);
        List<List<Vector2>> finalRoomVertices = new List<List<Vector2>>();
        finalRoomVertices.Add(LayoutUtil.GetVerticesFromLineSegments(baseRoom));

        //Get the sites neighboring the base room
        List<Vector2> neighborSites = voronoi.NeighborSitesForSite(roomOriginSite);

        //Add neighboring cells to the final room until the required room size has been met.
        //NOTE: Currently adds duplicate vertices to the final room in order to be able to triangulate each room-piece seperately later on.
        for (int i = 0; i < size - 1; i++)
        {
            //Start adding neighbors
            List<LineSegment> neighbor = null;
            if (i < neighborSites.Count)
                neighbor = voronoi.VoronoiBoundaryForSite(neighborSites[i]);

            //When no more neighbors are available, start adding the neighbors' neighbors
            else
            {
                bool found = false;
                Vector2 newCoord = Vector2.zero;

                //Find a neighboring site that has available neighbors
                foreach (Vector2 site in neighborSites)
                {
                    List<Vector2> neighbors = voronoi.NeighborSitesForSite(site);
                    foreach (Vector2 n in neighbors)
                    {
                        if (n != roomOriginSite && !neighborSites.Contains(n))
                        {
                            newCoord = n;
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }

                //Set the neighbor and add it to the neighborSites list so it can be used to find more neighbors
                if (found)
                {
                    neighbor = voronoi.VoronoiBoundaryForSite(newCoord);
                    neighborSites.Add(newCoord);
                }
                else
                {
                    Debug.LogWarning("Could not find a neighboring site at iteration " + i);
                    return null;
                }
            }

            //Add the found available neighbor to the room
            finalRoomVertices.Add(LayoutUtil.GetVerticesFromLineSegments(neighbor));
        }

        //Sort all room piece vertices clockwise for triangulation
        List<List<Vector2>> clockwiseVertices = new List<List<Vector2>>();
        foreach (List<Vector2> vertexSet in finalRoomVertices)
        {
            clockwiseVertices.Add(VectorUtil.SortClockwise(vertexSet));
        }

        return clockwiseVertices;
    }
   

    #region Debug

    private void OnDrawGizmos()
    {
        if (voronoi == null)
            return;

        if (drawVoronoi)
            DrawVoronoi();
        if (drawDelaunay)
            DrawDelaunay();
        if (drawSpanningTree)
            DrawSpanningTree();

        DrawLayout();
    }

    private void DrawLineSegments(List<LineSegment> segments, Color color)
    {
        Gizmos.color = color;
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2 left = (Vector2)segments[i].p0;
            Vector2 right = (Vector2)segments[i].p1;
            Gizmos.DrawLine((Vector3)left, (Vector3)right);
        }
    }

    private void DrawLayout()
    {
        if (_layout == null)
            return;
        Gizmos.color = Color.blue;
        foreach (List<Vector2> piece in _layout)
        {
            for (int i = 0; i < piece.Count; i++)
            {
                Vector2 left;
                Vector2 right;
                if (i + 1 < piece.Count)
                {
                    left = piece[i];
                    right = piece[i + 1];
                }
                else
                {
                    left = piece[i];
                    right = piece[0];
                }
                Gizmos.DrawLine(left, right);
            }
        }
    }

    private void DrawRandomVoronoiCell(string seed)
    {
        Vector2 coord = RandomUtil.RandomElement(voronoi.SiteCoords(), false, seed);
        List<LineSegment> cell = voronoi.VoronoiBoundaryForSite(coord);
        DrawLineSegments(cell, Color.blue);
    }

    private void DrawVoronoi()
    {
        List<LineSegment> diagram = voronoi.VoronoiDiagram();
        DrawLineSegments(diagram, Color.white);
    }

    private void DrawDelaunay()
    {
        List<LineSegment> delaunay = voronoi.DelaunayTriangulation();
        DrawLineSegments(delaunay, Color.red);
    }

    private void DrawSpanningTree()
    {
        List<LineSegment> tree = voronoi.SpanningTree();

        if (tree != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < tree.Count; i++)
            {
                LineSegment seg = tree[i];
                Vector2 left = (Vector2)seg.p0;
                Vector2 right = (Vector2)seg.p1;
                Gizmos.DrawLine((Vector3)left, (Vector3)right);
            }
        }
    }
    #endregion
}
