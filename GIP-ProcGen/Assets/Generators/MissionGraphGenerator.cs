using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For now, create linear graphs.
/// </summary>
public class MissionGraphGenerator : MonoBehaviour
{
    [Tooltip("The amount of challenges (lock-key) in the mission.")]
    public int challengeAmount = 3;

    [Tooltip("The amount of exploration nodes in the mission.")]
    public int explorationAmount = 3;

    [Tooltip("These mechanics are used to create challenges.")]
    public Mechanics[] availableMechanics;

    [Range(1, 4)]
    public int maxMechanicsPerChallenge = 1;

    private Graph<MissionNodeData> missionGraph;
    private GraphNode<MissionNodeData> entranceNode;
    private GraphNode<MissionNodeData> goalNode;
    private List<GraphNode<MissionNodeData>> availableKeyNodes = new List<GraphNode<MissionNodeData>>();
    private List<GraphNode<MissionNodeData>> availableLockNodes = new List<GraphNode<MissionNodeData>>();
    private List<GraphNode<MissionNodeData>> availableExplorationNodes = new List<GraphNode<MissionNodeData>>();
    private List<int> placedKeys = new List<int>();

    private void Start()
    {
        GenerateMissionGraph();
    }

    public void GenerateMissionGraph()
    {
        missionGraph = new Graph<MissionNodeData>();

        // Create a soup of all nodes required in this graph
        AddEntranceNode();
        AddChallengeNodes();
        AddExplorationNodes();
        AddGoalNode();

        // Connect the nodes in a way that makes sense
        ConnectAllNodes();

        // Print the final graph for debugging
        PrintMissionGraph();
    }

    // SOMEWHERE IN HERE IS A BUG THAT CAUSES IT TO PRINT THE GOAL NODE TWICE AND SKIP THE FINAL LOCK
    public void PrintMissionGraph()
    {
        bool endReached = false;
        bool foundNextNode = false;
        GraphNode<MissionNodeData> nextNode = entranceNode;
        List<GraphNode<MissionNodeData>> checkedNodes = new List<GraphNode<MissionNodeData>>();

        while (!endReached)
        {
            foundNextNode = false;

            // Print the node and mark is as checked
            Debug.Log(nextNode.Value);
            checkedNodes.Add(nextNode);

            // Since the graph is currently always linear (each node has only 1 or 2 connections), debug it linearly
            foreach (GraphNode<MissionNodeData> neighbor in nextNode.Neighbors)
            {
                if (!checkedNodes.Contains(neighbor))
                {
                    nextNode = neighbor;
                    foundNextNode = true;
                    break;
                }
            }


            // Mark the end of the graph
            if (!foundNextNode)
                endReached = true;

                       
        }
    }

    /// <summary>
    /// Add the entrance node to the graph.
    /// </summary>
    private void AddEntranceNode()
    {
        MissionNodeData entrance = new MissionNodeData(MissionNodeTypes.ENTRANCE);
        missionGraph.AddNode(entrance);
        entranceNode = (GraphNode<MissionNodeData>)missionGraph.Nodes.FindByValue(entrance);
    }

    /// <summary>
    /// Add the goal node to the graph.
    /// </summary>
    private void AddGoalNode()
    {
        MissionNodeData goal = new MissionNodeData(MissionNodeTypes.GOAL);
        missionGraph.AddNode(goal);
        goalNode = (GraphNode<MissionNodeData>)missionGraph.Nodes.FindByValue(goal);

        string debug = "";
        foreach (GraphNode<MissionNodeData> node in missionGraph.Nodes)
        {
            debug += node.Value.type + ", ";
        }
        Debug.Log("These nodes appear in the graph: " + debug);
    }

    /// <summary>
    /// Add all locks and keys to the graph
    /// </summary>
    private void AddChallengeNodes()
    {
        // Generate a key and a lock
        for (int i = 0; i < challengeAmount; i++)
        {
            // Create a key node and assign a set of mechanics that will be used for the challenge the player will need to beat in order to get the key
            MissionNodeData keyData = new MissionNodeData(MissionNodeTypes.KEY, i, GetRandomMechanicsSet());
            missionGraph.AddNode(keyData);
            availableKeyNodes.Add((GraphNode<MissionNodeData>)missionGraph.Nodes.FindByValue(keyData));

            // Create a lock node with the same key number
            MissionNodeData lockData = new MissionNodeData(MissionNodeTypes.LOCK, i);
            missionGraph.AddNode(lockData);
            availableLockNodes.Add((GraphNode<MissionNodeData>)missionGraph.Nodes.FindByValue(lockData));
        }
    }

    /// <summary>
    /// A random selection of available mechanics. Duplicate mechanics can occur.
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

    private void AddExplorationNodes()
    {
        for (int i = 0; i < explorationAmount; i++)
        {
            MissionNodeData data = new MissionNodeData(MissionNodeTypes.EXPLORATION);
            missionGraph.AddNode(data);
            availableExplorationNodes.Add((GraphNode<MissionNodeData>)missionGraph.Nodes.FindByValue(data));
        }
    }

    private void ConnectAllNodes()
    {
        GraphNode<MissionNodeData> currentNode = entranceNode;
        GraphNode<MissionNodeData> nextNode;

        // Create a linear graph by simply connecting the current node with the next node.
        while (GetAvailableNodeCount() > 0)
        {
            nextNode = GetNextNode();
            missionGraph.AddUndirectedEdge(currentNode, nextNode, 1);
            currentNode = nextNode;
        }

        nextNode = goalNode;
        missionGraph.AddUndirectedEdge(currentNode, nextNode, 1);
    }

    /// <summary>
    /// The amount of nodes available for placement, exluding entrance and goal.
    /// </summary>
    /// <returns></returns>
    private int GetAvailableNodeCount()
    {
        return availableExplorationNodes.Count + availableKeyNodes.Count + availableLockNodes.Count;
    }

    /// <summary>
    /// Get a node from one of the 3 'available' node pools. The returned node will be marked as being placed in the graph.
    /// </summary>
    /// <returns></returns>
    private GraphNode<MissionNodeData> GetNextNode()
    {
        GraphNode<MissionNodeData> node = null;
        List<GraphNode<MissionNodeData>> nodeList = null;

        // Select the node list to pick from \\
        // First try to get a random exploration node
        if (availableExplorationNodes.Count > availableKeyNodes.Count)
        {
            nodeList = availableExplorationNodes;
            node = RandomUtil.RandomElement(nodeList.ToArray());
        }
        // Then try to get a random key node
        else if (availableKeyNodes.Count >= availableLockNodes.Count)
        {
            nodeList = availableKeyNodes;
            node = RandomUtil.RandomElement(nodeList.ToArray());
            placedKeys.Add(node.Value.keyNr);
        }
        // Then try to get a random lock of which the key has already been placed
        else
        {
            nodeList = availableLockNodes;

            // Shuffle the locks around so they don't always appear in chronological order
            System.Random random = new System.Random(System.DateTime.Now.GetHashCode());
            random.Shuffle(nodeList);

            // Grab the first lock that has its key placed
            foreach (GraphNode<MissionNodeData> lockNode in availableLockNodes)
            {
                if (placedKeys.Contains(lockNode.Value.keyNr))
                {
                    node = lockNode;
                    break;
                }
            }
        }

        // Remove the node from its list.
        nodeList.Remove(node);

        return node;
    }

}
