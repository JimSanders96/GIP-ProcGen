using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryGenerator : MonoBehaviour
{

    public Material material;

    /// <summary>
    /// Generates a mesh based on multiple vertex sets.
    /// </summary>
    /// <param name="pointsList"></param>
    public void GenerateMesh(List<List<Vector2>> pointsList)
    {
        // Init mesh object
        GameObject go = new GameObject();
        go.name = "Mesh - Multiple vertex sets";
        MeshFilter mf = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshCollider>();
        go.AddComponent<MeshRenderer>();
        Mesh mesh = mf.mesh;
        go.GetComponent<MeshRenderer>().material = material;

        // Init triangulation variables
        List<List<Vector2>> holes = new List<List<Vector2>>();
        List<int> indicesTotal = new List<int>();
        List<Vector3> verticesTotal = new List<Vector3>();
        int vertexCount = 0;

        // Add all points lists (cells from the voronoi grid) seperately to the vertex and indices lists.
        foreach (List<Vector2> points in pointsList)
        {
            List<int> indices = null;
            List<Vector3> vertices = null;

            //Triangulate
            Triangulator.Triangulate(points, holes, out indices, out vertices);

            //Account for previously processed vertices
            List<int> actualIndices = new List<int>();
            foreach (int index in indices)
            {
                actualIndices.Add(index + vertexCount);
            }
            vertexCount += vertices.Count;

            //Add vertexes and indexes (triangles) to the total
            indicesTotal.AddRange(actualIndices);
            verticesTotal.AddRange(vertices);
        }

        //Set mesh variables
        mesh.Clear();
        mesh.vertices = verticesTotal.ToArray();
        mesh.triangles = indicesTotal.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Set mesh collider
        go.GetComponent<MeshCollider>().sharedMesh = mesh;

        //Set UVs
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
        }
        mesh.uv = uvs;
    }

    /// <summary>
    /// Generates a mesh based on a single set of vertices.
    /// </summary>
    /// <param name="points"></param>
    public void GenerateMesh(List<Vector2> points)
    {
        // Init
        GameObject go = new GameObject();
        go.name = "Mesh - Single vertex set";
        MeshFilter mf = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshCollider>();
        go.AddComponent<MeshRenderer>();
        Mesh mesh = mf.mesh;
        go.GetComponent<MeshRenderer>().material = material;

        List<List<Vector2>> holes = new List<List<Vector2>>();
        List<int> indices = null;
        List<Vector3> vertices = null;

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


}
