using Delaunay.Geo;
using Delaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGenerator : MonoBehaviour
{

    [SerializeField]
    private string randomSeed = "lelele";
    [SerializeField]
    private int pointCount = 150;
    [SerializeField]
    private int width = 1000;
    [SerializeField]
    private int height = 1000;
    [SerializeField]
    private bool drawVoronoi = true, drawDelaunay = false, drawSpanningTree = false;

    private System.Random random;
    private Voronoi voronoi;
    private List<List<Vector2>> _layout;

    /// <summary>
    /// Generate the level layout based on a voronoi diagram and level parameters.
    /// ++ Currently returns a single room selected from a voronoi diagram ++
    /// </summary>
    /// <returns></returns>
    public List<List<Vector2>> GenerateLayout(int roomSize, int roomCount)
    {
        voronoi = GenerateVoronoiObject();
        List<List<Vector2>> layout = new List<List<Vector2>>();

        for (int i = 0; i < roomCount; i++)
        {
            layout.AddRange(GenerateRoom(voronoi, roomSize, randomSeed + i));
        }
        Debug.Log("Pieces in layout: " + layout.Count);
        _layout = layout;
        return layout;
    }

    /// <summary>
    /// Generate a room by selecting a random cell from a given voronoi grid and adding the surrounding cells
    /// to it until the required size has been met.
    /// </summary>
    /// <param name="voronoi"></param>
    /// <returns></returns>
    public List<List<Vector2>> GenerateRoom(Voronoi voronoi, int size, string seed)
    {
        if (size < 1)
            return null;

        //Select a random site from the voronoi diagram
        Vector2 coord = RandomUtil.RandomElement(voronoi.SiteCoords(), false, seed);

        //Start building the final room from the cell at the coord
        List<LineSegment> baseRoom = voronoi.VoronoiBoundaryForSite(coord);
        List<List<Vector2>> finalRoomVertices = new List<List<Vector2>>();
        finalRoomVertices.Add(GetVerticesFromLineSegments(baseRoom));

        //Get the sites neighboring the base room
        List<Vector2> neighborSites = voronoi.NeighborSitesForSite(coord);

        //Add neighboring cells to the final room until the required room size has been met.
        //WARNING: Currently adds duplicate vertices to the final room in order to be able to triangulate each room-piece seperately later on.
        for (int i = 0; i < size - 1; i++)
        {
            List<LineSegment> neighbor = voronoi.VoronoiBoundaryForSite(neighborSites[i]);
            finalRoomVertices.Add(GetVerticesFromLineSegments(neighbor));
        }

        //Sort all room piece vertices clockwise for triangulation
        List<List<Vector2>> clockwiseVertices = new List<List<Vector2>>();
        foreach (List<Vector2> vertexSet in finalRoomVertices)
        {
            clockwiseVertices.Add(VectorUtil.SortClockwise(vertexSet));
        }

        return clockwiseVertices;
    }

    /// <summary>
    /// Returns a list of all vertices in the lineSegments. 
    /// Filters out duplicate vertices.
    /// </summary>
    /// <param name="lineSegments"></param>
    /// <returns></returns>
    private List<Vector2> GetVerticesFromLineSegments(List<LineSegment> lineSegments)
    {
        List<Vector2> vertices = new List<Vector2>();

        // Get vertices from line segments.
        foreach (LineSegment line in lineSegments)
        {
            Vector2 p0 = (Vector2)line.p0;
            Vector2 p1 = (Vector2)line.p1;

            // Filter duplicate vertices
            if (!vertices.Contains(p0))
                vertices.Add(p0);
            if (!vertices.Contains(p1))
                vertices.Add(p1);
        }

        return vertices;
    }

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
    /// Generates a voronoi object based on the width and height parameters of this class.
    /// </summary>
    /// <returns></returns>
    private Voronoi GenerateVoronoiObject()
    {
        List<Vector2> points = new List<Vector2>();
        List<uint> colors = new List<uint>();

        for (int i = 0; i < pointCount; i++)
        {
            colors.Add(0);
            points.Add(new Vector2(
                    GetRandom().Next(0, width),
                    GetRandom().Next(0, height))
            );
        }

        return new Voronoi(points, colors, new Rect(0, 0, width, height));
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
