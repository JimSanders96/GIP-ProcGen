using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionGraphGenerator : MonoBehaviour
{
    [Tooltip("The amount of challenges in the mission.")]
    public int missionLength = 3;

    [Range(1, 3)]
    [Tooltip("Higher intensity means less exploration (cooldown) space between challenges.")]
    public int intensity = 1;

    [Tooltip("These mechanics are used to create challenges.")]
    public Mechanics[] availableMechanics;

    [Range(1,4)]
    public int maxMechanicsPerChallenge;

    private Graph<MissionNodeData> missionGraph;

    public void GenerateMissionGraph()
    {
        missionGraph = new Graph<MissionNodeData>();

        // Create a soup of all nodes required in this graph
        AddEntranceNode();
        AddChallengeNodes();
        AddGoalNode();

        // Connect the nodes in a way that makes sense
        ConnectAllNodes();

        // Print the final graph for debugging
        PrintMissionGraph();
    }

    public void PrintMissionGraph()
    {

    }

    private void AddEntranceNode()
    {
        MissionNodeData entrance = new MissionNodeData(MissionNodeTypes.ENTRANCE);
        missionGraph.AddNode(entrance);
    }

    private void AddGoalNode()
    {
        MissionNodeData goal = new MissionNodeData(MissionNodeTypes.GOAL);
        missionGraph.AddNode(goal);
    }

    private void AddChallengeNodes()
    {
        // Generate a key and a lock
        for (int i = 0; i < missionLength; i++)
        {
            // Create a key node and assign a set of mechanics that will be used for the challenge the player will need to beat in order to get the key
            MissionNodeData keyData = new MissionNodeData(MissionNodeTypes.KEY, i, GetRandomMechanicsSet());
            missionGraph.AddNode(keyData);

            // Create a lock node with the same key number
            MissionNodeData lockData = new MissionNodeData(MissionNodeTypes.LOCK, i);
            missionGraph.AddNode(lockData);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Mechanics[] GetRandomMechanicsSet()
    {
        int amount = Random.Range(1, maxMechanicsPerChallenge);
        Mechanics[] mechanics = new Mechanics[amount];

        for (int i = 0; i < amount; i++)
            mechanics[i] = RandomUtil.RandomElement(availableMechanics);

        return mechanics;
    }

    private void ConnectAllNodes()
    {

    }
}
