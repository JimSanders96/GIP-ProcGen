﻿using Delaunay.Geo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GeometryGenerator), typeof(LayoutGenerator))]
public class LevelGenerator : MonoBehaviour
{

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
        List<LineSegment> layout = layoutGenerator.GenerateLayout();
        List<Vector2> vertices = new List<Vector2>();

        // Get vertices from line segments.
        foreach (LineSegment line in layout)
        {
            Vector2 p0 = (Vector2)line.p0;
            Vector2 p1 = (Vector2)line.p1;

            if (!vertices.Contains(p0))
                vertices.Add(p0);
            if (!vertices.Contains(p1))
                vertices.Add(p1);
        }

        // TEMP
        vertices.Clear();
        vertices.Add(new Vector2(0, 1));
        vertices.Add(new Vector2(0, 0));
        vertices.Add(new Vector2(1, 1));
        vertices.Add(new Vector2(1, 0));

        geometryGenerator.GenerateMesh(vertices.ToArray());
    }
}