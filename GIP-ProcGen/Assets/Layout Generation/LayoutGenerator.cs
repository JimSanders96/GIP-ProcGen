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
    private int smoothingFactor = 1;
    [SerializeField]
    private int pointCount = 150;
    [SerializeField]
    private int width = 1000;
    [SerializeField]
    private int height = 1000;
    [SerializeField]
    private System.Random random;
    [SerializeField]
    private bool drawVoronoi, drawDelaunay, drawSpanningTree;

    private Voronoi voronoi;

    // Return the same random every time this is called
    private System.Random GetRandom()
    {
        if (random == null)
            random = new System.Random(seed.GetHashCode());
        return random;
    }

    private void Awake()
    {
        voronoi = GenerateVoronoiDiagram();
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
        if (drawVoronoi)
            DrawVoronoi();
        if (drawDelaunay)
            DrawDelaunay();
        if (drawSpanningTree)
            DrawSpanningTree();
    }
    #region Debug
    private void DrawVoronoi()
    {
        if (voronoi == null)
            return;

        List<LineSegment> voronoiDiagram = voronoi.VoronoiDiagram();
        if (voronoiDiagram != null)
        {
            Gizmos.color = Color.white;
            for (int i = 0; i < voronoiDiagram.Count; i++)
            {
                Vector2 left = (Vector2)voronoiDiagram[i].p0;
                Vector2 right = (Vector2)voronoiDiagram[i].p1;
                Gizmos.DrawLine((Vector3)left, (Vector3)right);
            }
        }
    }

    private void DrawDelaunay()
    {
        if (voronoi == null)
            return;

        List<LineSegment> delaunay = voronoi.DelaunayTriangulation();

        Gizmos.color = Color.magenta;
        if (delaunay != null)
        {
            for (int i = 0; i < delaunay.Count; i++)
            {
                Vector2 left = (Vector2)delaunay[i].p0;
                Vector2 right = (Vector2)delaunay[i].p1;
                Gizmos.DrawLine((Vector3)left, (Vector3)right);
            }
        }
    }

    private void DrawSpanningTree()
    {
        if (voronoi == null)
            return;

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
