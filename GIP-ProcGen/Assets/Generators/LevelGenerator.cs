using Delaunay.Geo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GeometryGenerator), typeof(LayoutGenerator))]
public class LevelGenerator : MonoBehaviour
{
    [Range(1,15)]
    public int roomSize = 1;
    [Range(1,5)]
    public int roomCount = 1;

    private GeometryGenerator geometryGenerator;
    private LayoutGenerator layoutGenerator;

    // Use this for initialization
    void Awake()
    {
        geometryGenerator = GetComponent<GeometryGenerator>();
        layoutGenerator = GetComponent<LayoutGenerator>();
        GenerateLevel();
    }

    void GenerateLevel()
    {
        List<List<Vector2>> layout = layoutGenerator.GenerateLayout(roomSize, roomCount);
        if(layout == null)
        {
            Debug.LogError("Could not generate layout!");
            return;
        }
        geometryGenerator.GenerateMesh(layout);
    }
}
