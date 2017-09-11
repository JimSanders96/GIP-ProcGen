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
    private System.Random random;
    [SerializeField]
    private bool drawVoronoi, drawDelaunay, drawSpanningTree, drawRandomCell = false;

    [SerializeField]
    private int roomCount = 1;
    [SerializeField]
    private int minRoomSize, maxRoomSize;

    private Voronoi voronoi;

    public List<LineSegment> GenerateLayout()
    {
        List<LineSegment> layout = null;

        //TEST
        voronoi = GenerateVoronoiDiagram();
        Vector2 coord = RandomUtil.RandomElement(voronoi.SiteCoords(), false, seed);
        layout = voronoi.VoronoiBoundaryForSite(coord);

        return layout;
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
        if (drawRandomCell)
        {
            for (int i = 0; i < roomCount; i++)
                DrawRandomVoronoiCell(seed);
        }
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

    private void DrawRandomVoronoiCell(string seed)
    {
        Vector2 coord = RandomUtil.RandomElement(voronoi.SiteCoords(), false, seed);
        List<LineSegment> cell = voronoi.VoronoiBoundaryForSite(coord);
        DrawLineSegments(cell, Color.blue);
    }

    private void DrawVoronoi()
    {
        List<LineSegment> voronoiDiagram = voronoi.VoronoiDiagram();
        DrawLineSegments(voronoiDiagram, Color.white);
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
