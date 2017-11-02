using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For now, create linear graphs.
/// </summary>
public class MissionGraphGenerator : MonoBehaviour
{
    public bool linearGraph = true;

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

    public Graph<MissionNodeData> GenerateMissionGraph()
    {
        missionGraph = new Graph<MissionNodeData>();

        // Create all nodes required in the graph. they will be stored in their respective variables.
        CreateEntranceNode();
        CreateChallengeNodes();
        CreateExplorationNodes();
        CreateGoalNode();

        // Connect the nodes in a way that makes sense
        CreateGraph(linearGraph);

        // Print the final graph for debugging
        // PrintMissionGraph();

        // Return the graph
        return missionGraph;
    }

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
    private void CreateEntranceNode()
    {
        MissionNodeData entrance = new MissionNodeData(MissionNodeTypes.ENTRANCE);
        entranceNode = new GraphNode<MissionNodeData>(entrance);
    }

    /// <summary>
    /// Add the goal node to the graph.
    /// </summary>
    private void CreateGoalNode()
    {
        MissionNodeData goal = new MissionNodeData(MissionNodeTypes.GOAL);
        goalNode = new GraphNode<MissionNodeData>(goal);

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
    private void CreateChallengeNodes()
    {
        // Generate a key and a lock
        for (int i = 0; i < challengeAmount; i++)
        {
            // Create a key node and assign a set of mechanics that will be used for the challenge the player will need to beat in order to get the key
            MissionNodeData keyData = new MissionNodeData(MissionNodeTypes.KEY, i, GetRandomMechanicsSet());
            availableKeyNodes.Add(new GraphNode<MissionNodeData>(keyData));

            // Create a lock node with the same key number
            MissionNodeData lockData = new MissionNodeData(MissionNodeTypes.LOCK, i);
            availableLockNodes.Add(new GraphNode<MissionNodeData>(lockData));
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

    private void CreateExplorationNodes()
    {
        for (int i = 0; i < explorationAmount; i++)
        {
            MissionNodeData data = new MissionNodeData(MissionNodeTypes.EXPLORATION);
            availableExplorationNodes.Add(new GraphNode<MissionNodeData>(data));
        }
    }

    private void CreateGraph(bool linear = true)
    {
        if (linear)
        {
            GraphNode<MissionNodeData> currentNode = entranceNode;
            GraphNode<MissionNodeData> nextNode;
            missionGraph.AddNode(currentNode);

            // Create a linear graph by simply connecting the current node with the next node.
            while (GetAvailableNodeCount() > 0)
            {
                nextNode = GetNextAvailableNode();
                missionGraph.AddNode(nextNode);
                missionGraph.AddUndirectedEdge(currentNode, nextNode, GetRandomEdgeWeight());
                currentNode = nextNode;
            }

            nextNode = goalNode;
            missionGraph.AddNode(nextNode);
            missionGraph.AddUndirectedEdge(currentNode, nextNode, GetRandomEdgeWeight());

        }
        else
        {
            // Create first connection
            missionGraph.AddNode(entranceNode);
            GraphNode<MissionNodeData> nextNode = GetNextAvailableNode();
            missionGraph.AddNode(nextNode);
            missionGraph.AddUndirectedEdge(entranceNode, nextNode, GetRandomEdgeWeight());

            // Create a non-linear graph by connecting the next nodes to a random node
            while (GetAvailableNodeCount() > 0)
            {
                nextNode = GetNextAvailableNode();
                missionGraph.AddNode(nextNode);
                bool success = ConnectNodeRandomly(nextNode);
                Debug.Log(success);
            }

            // Randomly connect the goal node
            missionGraph.AddNode(goalNode);
            ConnectNodeRandomly(goalNode);
        }
    }

    private bool ConnectNodeRandomly(GraphNode<MissionNodeData> node)
    {
        // Get all nodes in the current graph that the input node can be connected to
        List<GraphNode<MissionNodeData>> linkableGraphNodes = GetLinkableMissionGraphNodes();
        linkableGraphNodes.Remove(node);

        // Fail if no linkable graph nodes available
        if (linkableGraphNodes.Count == 0)
            return false;

        GraphNode<MissionNodeData> randomNode = linkableGraphNodes[Random.Range(0, linkableGraphNodes.Count)];

        // Connect the node
        missionGraph.AddUndirectedEdge(randomNode, node, GetRandomEdgeWeight());
        return true;

    }

    private List<GraphNode<MissionNodeData>> GetLinkableMissionGraphNodes(int maxConnections = 3)
    {
        List<GraphNode<MissionNodeData>> nodes = new List<GraphNode<MissionNodeData>>();
        foreach (GraphNode<MissionNodeData> node in missionGraph.Nodes)
        {
            if (node.Neighbors.Count < maxConnections)
                nodes.Add(node);
        }

        return nodes;
    }


    /// <summary>
    /// Return a random number between 1 and 2.
    /// This number can be used to define the type of connection between mission nodes.
    /// </summary>
    /// <returns></returns>
    private int GetRandomEdgeWeight()
    {
        return Random.Range(1, 2);
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
    private GraphNode<MissionNodeData> GetNextAvailableNode()
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
