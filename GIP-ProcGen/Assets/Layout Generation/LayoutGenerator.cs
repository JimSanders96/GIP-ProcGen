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
    private int pointCount = 150;
    [SerializeField]
    private int width = 1000;
    [SerializeField]
    private int height = 1000;
    [SerializeField]
    private int roomSize = 1;
    [SerializeField]
    private bool drawVoronoi, drawDelaunay, drawSpanningTree;

    private System.Random random;
    private Voronoi voronoi;
    private List<LineSegment> _layout;

    /// <summary>
    /// Generate the level layout based on a voronoi diagram and level parameters.
    /// ++ Currently returns a single room selected from a voronoi diagram ++
    /// </summary>
    /// <returns></returns>
    public List<LineSegment> GenerateLayout()
    {
        voronoi = GenerateVoronoiDiagram();
        List<LineSegment> layout = GenerateRoom(voronoi, roomSize);
        _layout = layout;
        return layout;
    }

    /// <summary>
    /// Generate a room by selecting a random cell from a given voronoi grid and adding the surrounding cells
    /// to it until the required size has been met.
    /// </summary>
    /// <param name="voronoi"></param>
    /// <returns></returns>
    public List<LineSegment> GenerateRoom(Voronoi voronoi, int size)
    {
        if (size < 1)
            return null;

        //Select a random site from the voronoi diagram
        Vector2 coord = RandomUtil.RandomElement(voronoi.SiteCoords(), false, seed);

        //Start building the final room from the cell at the coord
        List<LineSegment> finalRoom = voronoi.VoronoiBoundaryForSite(coord);

        //Get the sites neighboring the selected cell
        List<Vector2> neighborSites = voronoi.NeighborSitesForSite(coord);

        //Add neighboring cells to the final room until the required room size has been met.
        //If a LineSegment already exists within the room, don't add it.
        for (int i = 0; i < size - 1; i++)
        {
            List<LineSegment> neighbor = voronoi.VoronoiBoundaryForSite(neighborSites[i]);
            foreach (LineSegment line in neighbor)
                if (!finalRoom.Contains(line))
                finalRoom.Add(line);
        }

        //TODO: Remove inner LineSegments / find the convex hull
        // Currently, the geometry generator aligns vertices clockwise. This, however, does not correspond to the final room in case of size > 1.

        return finalRoom;
    }

    // Return the same random every time this is called
    private System.Random GetRandom()
    {
        if (random == null)
            random = new System.Random(seed.GetHashCode());
        return random;
    }

    private Voronoi GenerateVoronoiDiagram()
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
    #region Debug

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
        List<LineSegment> l = _layout;
        if (l == null)
            return;

        DrawLineSegments(l, Color.blue);
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
