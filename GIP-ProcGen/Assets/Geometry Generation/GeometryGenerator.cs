using Hydra.HydraCommon.Utils.Comparers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryGenerator : MonoBehaviour
{

    public Material material;

    public void GenerateMesh(List<Vector2> points)
    {
        // Init
        GameObject go = new GameObject();
        go.name = "Mesh";
        MeshFilter mf = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshCollider>();
        go.AddComponent<MeshRenderer>();
        Mesh mesh = mf.mesh;
        go.GetComponent<MeshRenderer>().material = material;

        List<List<Vector2>> holes = new List<List<Vector2>>();
        List<int> indices = null;
        List<Vector3> vertices = null;

        // Sort the points clockwise        
        Vector2[] pointsArray = points.ToArray();
        Vector2 origin = FindOrigin(pointsArray);
        Array.Sort(pointsArray, new ClockwiseComparer(origin));
        points = new List<Vector2>(pointsArray);

        //Triangulate
        Triangulator.Triangulate(points, holes, out indices, out vertices);

        //Set mesh
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        go.GetComponent<MeshCollider>().sharedMesh = mesh;

        //Set UVs
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
        }
        mesh.uv = uvs;
    }

    // Returns the center of a given array of points.
    Vector2 FindOrigin(Vector2[] points)
    {
        if (points.Length == 0)
            return Vector2.zero;
        if (points.Length == 1)
            return points[0];

        Vector2 origin = Vector2.zero;
        for (int i = 0; i < points.Length; i++)
            origin += points[i];

        return origin / points.Length;
    }
}
