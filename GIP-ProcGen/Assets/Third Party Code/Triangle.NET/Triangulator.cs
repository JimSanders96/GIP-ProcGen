using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;

public class Triangulator
{
    /// <summary>
    /// Triangulate a given set of points using the Triangle.NET library.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="holes"></param>
    /// <param name="outIndices"></param>
    /// <param name="outVertices"></param>
    /// <returns></returns>
    public static bool Triangulate(List<Vector2> points, List<List<Vector2>> holes, out List<int> outIndices, out List<Vector3> outVertices)
    {
        Polygon poly = new Polygon();
        outVertices = new List<Vector3>();
        outIndices = new List<int>();

        // Points and segments
        for (int i = 0; i < points.Count; i++)
        {
            poly.Add(new Vertex(points[i].x, points[i].y));

            // Close the polygon by linking the last vertex to the first
            if (i == points.Count - 1)
                poly.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[0].x, points[0].y)));
            // Link the current vertex to the next
            else
                poly.Add(new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[i + 1].x, points[i + 1].y)));
        }

        // Holes
        for (int i = 0; i < holes.Count; i++)
        {
            List<Vertex> vertices = new List<Vertex>();
            for (int j = 0; j < holes[i].Count; j++)
            {
                vertices.Add(new Vertex(holes[i][j].x, holes[i][j].y));
            }
            poly.Add(new Contour(vertices), true);
        }

        // This can take quality parameters
        var mesh = poly.Triangulate();

        // Assign vertices and indices
        foreach (ITriangle t in mesh.Triangles)
        {
            for (int j = 2; j >= 0; j--)
            {
                bool found = false;
                for (int k = 0; k < outVertices.Count; k++)
                {
                    if((outVertices[k].x == t.GetVertex(j).X) && (outVertices[k].z == t.GetVertex(j).Y))
                    {
                        outIndices.Add(k);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    outVertices.Add(new Vector3((float)t.GetVertex(j).X, 0, (float)t.GetVertex(j).Y));
                    outIndices.Add(outVertices.Count - 1);
                }
            }
        }

        return true;
    }
}
