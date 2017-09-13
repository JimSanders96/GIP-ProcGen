using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryGenerator : MonoBehaviour
{

    public Material material;

    private int count = 0;

    public void GenerateMesh(List<List<Vector2>> pointsList)
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
            foreach(int index in indices)
            {
                actualIndices.Add(index + vertexCount);
            }
            vertexCount += vertices.Count;

            //Add vertexed and indexes (triangles) to the total
            indicesTotal.AddRange(actualIndices);
            verticesTotal.AddRange(vertices);
        }

        //Set mesh
        mesh.Clear();
        mesh.vertices = verticesTotal.ToArray();
        mesh.triangles = indicesTotal.ToArray();
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

    public void GenerateMesh(List<Vector2> points)
    {
        // Init
        GameObject go = new GameObject();
        go.name = "Mesh" + count;
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
