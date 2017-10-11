using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionGraphGenerator : MonoBehaviour
{
    [Tooltip("The amount of challenges in the mission")]
    public int missionLength = 3;

    [Tooltip("These mechanics are used to create challenges")]
    public Mechanics[] requiredMechanics;

    public void GenerateMission()
    {

    }
}
