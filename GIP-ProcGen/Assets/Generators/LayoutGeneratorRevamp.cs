using System.Collections;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay;
using UnityEngine;

public class LayoutGeneratorRevamp : MonoBehaviour
{
    public struct Room
    {
        public Vector2 gridCoord;
        public List<Vector2> siteCoords;
        public MissionNodeData missionNodeData;
    }

    public GameObject missionMarker;
    public GameObject debugCube;
    public GameObject debugCube2;
    [Range(10, 50)]
    public int gridSize = 50;
    public int sitesPerGridTile;

    private List<MissionNodeData> exploredMissionData;
    private List<Vector2> exploredGridCoords;
    private List<Room> missionRooms;
    private List<Room> fleshRooms;
    private List<List<Vector2>> layout;
    private Voronoi voronoi;


    private void Awake()
    {
        // FOCUS SCENE VIEW
        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
    }

    /// <summary>
    /// Generate a layout based on a mission graph.
    /// </summary>
    /// <param name="mission"></param>
    /// <returns></returns>
    public List<List<Vector2>> GenerateLayout(Graph<MissionNodeData> mission)
    {
        bool success = false;
        int attempts = 0;

        while (!success)
        {
            attempts++;

            // Init vars
            layout = new List<List<Vector2>>();
            exploredMissionData = new List<MissionNodeData>();
            exploredGridCoords = new List<Vector2>();
            missionRooms = new List<Room>();

            // Recursively generate rooms from the mission graph
            success = GenerateRoomFromMissionNode((GraphNode<MissionNodeData>)mission.Nodes[0], Vector2.zero);
            Debug.Log("All rooms should have been generated now");
        }

        // Generate random rooms around room sites to flesh out the voronoi
        CreateFleshRooms();

        // Normalize rooms before voronoi generation
        NormalizeRooms();

        // Generate the voronoi
        voronoi = GenerateVoronoi(GetVoronoiSites());

        // Get vertex sets from generated voronoi stuff
        layout = GetLayoutVertices(missionRooms, voronoi);

        // DEBUG ROOMS
        foreach (Room room in missionRooms)
            DebugRoom(room, 0);
        foreach (Room room in fleshRooms)
            DebugRoom(room, 1, false);

        Debug.Log("Generation completed in " + attempts + " attempts.");

        return layout;
    }

    /// <summary>
    /// Returns a set of vertex sets belonging to the mission rooms in a voronoi object.
    /// ROOM SITES MUST APPEAR IN THE VORONOI OR THE UNIVERSE WILL COLLAPSE
    /// </summary>
    /// <param name="missionRooms"></param>
    /// <param name="voronoi"></param>
    /// <returns></returns>
    private List<List<Vector2>> GetLayoutVertices(List<Room> missionRooms, Voronoi voronoi)
    {
        // Get all vertices belonging to the room sites
        List<List<Vector2>> vertexSets = new List<List<Vector2>>();
        foreach (Room room in missionRooms)
        {
            List<List<Vector2>> vertices = LayoutUtil.GetVerticesForSites(voronoi, room.siteCoords);
            vertexSets.AddRange(vertices);
        }

        //Sort all room piece vertices clockwise for triangulation
        List<List<Vector2>> clockwiseVertices = new List<List<Vector2>>();
        foreach (List<Vector2> vertexSet in vertexSets)
        {
            clockwiseVertices.Add(VectorUtil.SortClockwise(vertexSet));
        }

        return clockwiseVertices;
    }

    /// <summary>
    /// Generate a voronoi object based on a set of sites.
    /// </summary>
    /// <param name="sites"></param>
    /// <returns></returns>
    private Voronoi GenerateVoronoi(List<Vector2> sites)
    {
        int maxX = 0;
        int maxY = 0;

        // Find the highest grid coordinates to find the upper right corner of the voronoi (lower left is 0,0)
        foreach (Vector2 site in sites)
        {
            int x = (int)site.x;
            int y = (int)site.y;

            if (x > maxX)
                maxX = x;
            if (y > maxY)
                maxY = y;
        }

        return LayoutUtil.GenerateVoronoiObject(sites, maxX, maxY);
    }

    /// <summary>
    /// Returns all sites of every room (both mission and flesh)
    /// </summary>
    /// <returns></returns>
    private List<Vector2> GetVoronoiSites()
    {
        List<Vector2> sites = new List<Vector2>();

        foreach (Room room in missionRooms)
            sites.AddRange(room.siteCoords);
        foreach (Room room in fleshRooms)
            sites.AddRange(room.siteCoords);
        //sites.AddRange(GetUnusedGridCoords());

        return sites;
    }

    /// <summary>
    /// Returns all grid coordinates that do not exist within the exploredGridTiles bounds (using the max x and y coords from those)
    /// </summary>
    /// <returns></returns>
    private List<Vector2> GetUnusedGridCoords()
    {
        int maxX = 0;
        int maxY = 0;

        // Find the highest grid coordinates to find the upper right corner of the voronoi (lower left is 0,0)
        foreach (Vector2 site in exploredGridCoords)
        {
            int x = (int)site.x;
            int y = (int)site.y;

            if (x > maxX)
                maxX = x;
            if (y > maxY)
                maxY = y;
        }

        // Find all grid coords that havent been explored
        List<Vector2> unusedGridCoords = new List<Vector2>();
        for (int x = 0; x < maxX + 1; x++)
        {
            for (int y = 0; y < maxY + 1; y++)
            {
                Vector2 coord = new Vector2(x, y);
                if (!exploredGridCoords.Contains(coord))
                    unusedGridCoords.Add(coord);
            }
        }
        Debug.Log("lalala");
        return unusedGridCoords;
    }

    /// <summary>
    /// Create rooms in every grid tile that hasn't been used by a mission room to flesh out the voronoi
    /// </summary>
    private void CreateFleshRooms()
    {
        fleshRooms = new List<Room>();

        // Create rooms around mission rooms to create a decent border in the voronoi object later on. (no need to filter out duplicate coords)
        foreach (Room room in missionRooms)
            foreach (Vector2 neighbor in GetAllAvailableNeighborGridCoords(room.gridCoord))
                fleshRooms.Add(CreateRoom(neighbor, sitesPerGridTile));

    }


    /// <summary>
    /// Recursively generates rooms for the input node and all of its unexplored neighbors until all neighbors have been explored.
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="gridCoord"></param>
    private bool GenerateRoomFromMissionNode(GraphNode<MissionNodeData> startNode, Vector2 gridCoord)
    {
        // Extract mission and mark is as 'explored' to prevent infinite recursion.
        MissionNodeData data = startNode.Value;
        exploredMissionData.Add(data);

        // Create the room and store it
        Room room = CreateRoom(gridCoord, sitesPerGridTile);
        room.missionNodeData = data;
        missionRooms.Add(room);

        // Mark the input gridCoord as explored if it hasn't already been done 
        //(should only happen for the first room, as neighboring coords are being marked before entering this function in the next section)
        if (!exploredGridCoords.Contains(gridCoord))
            exploredGridCoords.Add(gridCoord);

        // Establish which neighbors to call this function for next
        Dictionary<GraphNode<MissionNodeData>, Vector2> nextNodes = new Dictionary<GraphNode<MissionNodeData>, Vector2>();
        foreach (GraphNode<MissionNodeData> neighbor in startNode.Neighbors)
        {
            if (!exploredMissionData.Contains(neighbor.Value))
            {
                // Find an available coord for the neighbor and mark it as explored, fail if none available
                Vector2 neighborCoord = GetAvailableNeighborGridCoord(gridCoord);
                if (neighborCoord == Vector2.zero)
                    return false;
                exploredGridCoords.Add(neighborCoord);

                // Make note of this neighbors coordinate
                nextNodes.Add(neighbor, neighborCoord);
            }
        }

        // Call this function for established neighbors
        foreach (KeyValuePair<GraphNode<MissionNodeData>, Vector2> entry in nextNodes)
        {
            bool success = GenerateRoomFromMissionNode(entry.Key, entry.Value);
            if (!success)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a list of points, representing the voronoi sites of a room.
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <param name="pointCount"></param>
    /// <returns></returns>
    private Room CreateRoom(Vector2 gridCoord, int pointsPerTile)
    {
        // Init vars
        List<Vector2> points = new List<Vector2>();
        Room room = new Room();
        if (missionRooms == null) missionRooms = new List<Room>();

        // Calculate point boundaries
        int minX, maxX, minY, maxY = 0;
        GetPointBoundaries(gridCoord, out minX, out maxX, out minY, out maxY);

        // Randomly distribute points across the selected grid tile
        for (int i = 0; i < pointsPerTile; i++)
        {
            Vector2 point = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            points.Add(point);
        }

        // Assign vars to room
        room.gridCoord = gridCoord;
        room.siteCoords = points;


        return room;
    }

    /// <summary>
    /// Returns a coord directly surrounding the start coord that isn't currently marked as 'explored'.
    /// </summary>
    /// <param name="startCoord"></param>
    /// <returns></returns>
    private Vector2 GetAvailableNeighborGridCoord(Vector2 startCoord)
    {
        // Get all surrounding coords (filter out used coords)
        List<Vector2> availableCoords = GetAllAvailableNeighborGridCoords(startCoord);

        // Return (0,0) and error when no neighbors available
        if (availableCoords.Count == 0)
        {
            Debug.LogWarning("No available neighbors for coord " + startCoord + "!");
            return Vector2.zero;
        }

        // Return a random entry from the available coords
        return RandomUtil.RandomElement(availableCoords.ToArray());
    }

    /// <summary>
    /// Get a list of all coordinates surrounding the startCoord that havent been explored yet.
    /// Limited to north east south west
    /// </summary>
    /// <param name="startCoord"></param>
    /// <returns></returns>
    private List<Vector2> GetAllAvailableNeighborGridCoords(Vector2 startCoord)
    {
        // Get all surrounding coords (filter out used coords)
        List<Vector2> availableCoords = new List<Vector2>();
        for (int x = (int)startCoord.x - 1; x < (int)startCoord.x + 2; x++)
        {
            for (int y = (int)startCoord.y - 1; y < (int)startCoord.y + 2; y++)
            {
                // Limit search to north east south west
                if (x == (int)startCoord.x || y == (int)startCoord.y)
                {
                    Vector2 coord = new Vector2(x, y);
                    if (coord != startCoord && !GridCoordIsTaken(coord))
                        availableCoords.Add(coord);
                }

            }
        }

        // Return a random entry from the available coords
        return availableCoords;
    }

    private void GetPointBoundaries(Vector2 gridCoord, out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = ((int)gridCoord.x * gridSize);
        minY = ((int)gridCoord.y * gridSize);
        maxX = ((int)(gridCoord.x + 1) * gridSize);
        maxY = ((int)(gridCoord.y + 1) * gridSize);
        //Debug.Log("Boundaries for gridCoord " + gridCoord + ": " + "minX: " + minX + " | minY: " + minY + " | maxX: " + maxX + " | maxY: " + maxY);
    }

    /// <summary>
    /// Returns true if the given gridCoord exists in the exploredGridCoords list.
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    private bool GridCoordIsTaken(Vector2 gridCoord)
    {
        return exploredGridCoords.Contains(gridCoord);
    }

    /// <summary>
    /// Finds the lowest X and Y site coordinates of all rooms and moves all sites so the lowest number is 0 or greater (required for voronoi creation).
    /// </summary>
    /// <returns></returns>
    private void NormalizeRooms()
    {
        List<Room> normalizedMissionRooms = new List<Room>();
        List<Room> normalizedFleshRooms = new List<Room>();

        int lowestX = 0;
        int lowestY = 0;

        // Find the lowest grid coordinates (because sites are places in an area north and east of the grid coord).
        foreach (Room room in fleshRooms)
        {
            int gridX = (int)room.gridCoord.x;
            int gridY = (int)room.gridCoord.y;

            if (gridX < lowestX)
                lowestX = gridX;
            if (gridY < lowestY)
                lowestY = gridY;
        }

        // Adjust coordinates for mission rooms
        foreach (Room room in missionRooms)
        {
            Room normalizedRoom = new Room();

            // adjust grid coord
            normalizedRoom.gridCoord = new Vector2(room.gridCoord.x - lowestX, room.gridCoord.y - lowestY);

            // adjust site coords
            List<Vector2> adjustedSites = new List<Vector2>();
            foreach (Vector2 site in room.siteCoords)
                adjustedSites.Add(new Vector2(site.x - (lowestX * gridSize), site.y - (lowestY * gridSize)));
            normalizedRoom.siteCoords = adjustedSites;

            // make sure the node data remains the same
            normalizedRoom.missionNodeData = room.missionNodeData;

            // add the room to the new list
            normalizedMissionRooms.Add(normalizedRoom);
        }

        // Adjust coordinates for flesh rooms
        foreach (Room room in fleshRooms)
        {
            Room normalizedRoom = new Room();

            // adjust grid coord
            normalizedRoom.gridCoord = new Vector2(room.gridCoord.x - lowestX, room.gridCoord.y - lowestY);

            // adjust site coords
            List<Vector2> adjustedSites = new List<Vector2>();
            foreach (Vector2 site in room.siteCoords)
                adjustedSites.Add(new Vector2(site.x - (lowestX * gridSize), site.y - (lowestY * gridSize)));
            normalizedRoom.siteCoords = adjustedSites;

            // make sure the node data remains the same
            normalizedRoom.missionNodeData = room.missionNodeData;

            // add the room to the new list
            normalizedFleshRooms.Add(normalizedRoom);
        }

        // Set actual rooms to normalized rooms.
        missionRooms = normalizedMissionRooms;
        fleshRooms = normalizedFleshRooms;
    }



    #region DEBUG
    private void DebugRoom(Room room, int type, bool placeMissionMarker = true)
    {
        foreach (Vector2 site in room.siteCoords)
        {
            Instantiate(type == 0 ? debugCube : debugCube2, new Vector3(site.x, 0, site.y), Quaternion.identity);
        }

        if (placeMissionMarker)
            PlaceMissionMarker(room);
    }

    /// <summary>
    /// Places a mission marker at the site location, displaying the data provided.
    /// </summary>
    /// <param name="site"></param>
    /// <param name="data"></param>
    private void PlaceMissionMarker(Room room)
    {
        Vector2 roomOrigin = VectorUtil.FindOrigin(room.siteCoords);
        GameObject marker = Instantiate(missionMarker, roomOrigin, Quaternion.identity);
        marker.GetComponent<MissionMarker>().Init(room.missionNodeData);
    }

    private void OnDrawGizmos()
    {
        if (voronoi == null)
            return;

        DrawVoronoi();
        DrawLayout();
    }

    private void DrawLineSegments(List<LineSegment> segments, Color color)
    {
        Gizmos.color = color;
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2 left = (Vector2)segments[i].p0;
            Vector2 right = (Vector2)segments[i].p1;
            Gizmos.DrawLine((Vector3)left, (Vector3)right);
        }
    }

    private void DrawLayout()
    {
        if (layout == null)
            return;
        Gizmos.color = Color.blue;
        foreach (List<Vector2> vertexSet in layout)
        {
            for (int i = 0; i < vertexSet.Count; i++)
            {
                Vector2 left;
                Vector2 right;
                if (i + 1 < vertexSet.Count)
                {
                    left = vertexSet[i];
                    right = vertexSet[i + 1];
                }
                else
                {
                    left = vertexSet[i];
                    right = vertexSet[0];
                }
                Gizmos.DrawLine(left, right);
            }
        }
    }

    private void DrawVoronoi()
    {
        List<LineSegment> diagram = voronoi.VoronoiDiagram();
        DrawLineSegments(diagram, Color.white);
    }
    #endregion
}
