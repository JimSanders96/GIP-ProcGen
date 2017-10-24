﻿using Delaunay.Geo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GeometryGenerator), typeof(LayoutGenerator), typeof(MissionGraphGenerator))]
public class LevelGenerator : MonoBehaviour
{
    [Range(3,50)]
    public int roomSize = 3;
    [Range(1,5)]
    public int roomCount = 1;

    private GeometryGenerator geometryGenerator;
    private LayoutGenerator layoutGenerator;
    private MissionGraphGenerator missionGraphGenerator;
    private Graph<MissionNodeData> missionGraph;

    // Use this for initialization
    void Awake()
    {
        geometryGenerator = GetComponent<GeometryGenerator>();
        layoutGenerator = GetComponent<LayoutGenerator>();
        missionGraphGenerator = GetComponent<MissionGraphGenerator>();

        GenerateLevel();
    }

    //test
    void GenerateLevel()
    {
        missionGraph = missionGraphGenerator.GenerateMissionGraph();
        //List<List<Vector2>> layout = layoutGenerator.GenerateLayout(roomSize, roomCount);
        List<List<Vector2>> layout = layoutGenerator.GenerateLayout(missionGraph,roomSize);

        if (layout == null)
        {
            Debug.LogError("Could not generate layout!");
            return;
        }
        geometryGenerator.GenerateMesh(layout);
    }
}
