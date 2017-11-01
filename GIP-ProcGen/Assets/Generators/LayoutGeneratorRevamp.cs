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
    [Range(10, 50)]
    public int gridSize = 50;
    public int sitesPerGridTile;

    private List<MissionNodeData> exploredMissionData;
    private List<Vector2> exploredGridCoords;
    private List<Room> rooms;
    private List<List<Vector2>> layout;
    private Voronoi voronoi;


    private void Awake()
    {

    }

    /// <summary>
    /// Generate a layout based on a mission graph.
    /// </summary>
    /// <param name="mission"></param>
    /// <returns></returns>
    public List<List<Vector2>> GenerateLayout(Graph<MissionNodeData> mission)
    {
        // Init vars
        layout = new List<List<Vector2>>();
        exploredMissionData = new List<MissionNodeData>();
        exploredGridCoords = new List<Vector2>();
        rooms = new List<Room>();

        // Recursively generate rooms from the mission graph
        GenerateRoomFromMissionNode((GraphNode<MissionNodeData>)mission.Nodes[0], Vector2.zero);
        Debug.Log("All rooms should have been generated now");

        // Normalize rooms
        rooms = NormalizeRooms(rooms);

        foreach (Room room in rooms)
        {
            DebugRoom(room);
        }

        return layout;
    }

    /// <summary>
    /// Recursively generates rooms for the input node and all of its unexplored neighbors until all neighbors have been explored.
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="gridCoord"></param>
    private void GenerateRoomFromMissionNode(GraphNode<MissionNodeData> startNode, Vector2 gridCoord)
    {
        // Extract mission and mark is as 'explored' to prevent infinite recursion.
        MissionNodeData data = startNode.Value;
        exploredMissionData.Add(data);

        // Create the room and store it
        Room room = CreateRoom(gridCoord, sitesPerGridTile);
        room.missionNodeData = data;
        rooms.Add(room);

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
                // Find an available coord for the neighbor and mark it as explored
                Vector2 neighborCoord = GetAvailableNeighborGridCoord(gridCoord);
                exploredGridCoords.Add(neighborCoord);

                // Make note of this neighbors coordinate
                nextNodes.Add(neighbor, neighborCoord);
            }
        }

        // Call this function for established neighbors
        foreach (KeyValuePair<GraphNode<MissionNodeData>, Vector2> entry in nextNodes)
        {
            GenerateRoomFromMissionNode(entry.Key, entry.Value);
        }
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
        if (rooms == null) rooms = new List<Room>();

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

        Debug.Log("Created room at grid tile " + room.gridCoord);
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
        List<Vector2> availableCoords = new List<Vector2>();
        for (int x = (int)startCoord.x - 1; x < (int)startCoord.x + 2; x++)
        {
            for (int y = (int)startCoord.y - 1; y < (int)startCoord.y + 2; y++)
            {
                Vector2 coord = new Vector2(x, y);
                if (coord != startCoord && !GridCoordIsTaken(coord))
                    availableCoords.Add(coord);
            }
        }

        // Return (0,0) and error when no neighbors available
        if (availableCoords.Count == 0)
        {
            Debug.LogError("No available neighbors for coord " + startCoord + "!");
            return Vector2.zero;
        }

        // Return a random entry from the available coords
        return RandomUtil.RandomElement(availableCoords.ToArray());
    }

    private void GetPointBoundaries(Vector2 gridCoord, out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = ((int)gridCoord.x * gridSize);
        minY = ((int)gridCoord.y * gridSize);
        maxX = ((int)(gridCoord.x + 1) * gridSize);
        maxY = ((int)(gridCoord.y + 1) * gridSize);
        Debug.Log("Boundaries for gridCoord " + gridCoord + ": " + "minX: " + minX + " | minY: " + minY + " | maxX: " + maxX + " | maxY: " + maxY);
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
    private List<Room> NormalizeRooms(List<Room> rooms)
    {
        List<Room> normalizedRooms = new List<Room>();
        int lowestX = 0;
        int lowestY = 0;

        // Find the lowest grid coordinates (because sites are places in an area north and east of the grid coord).
        foreach (Room room in rooms)
        {
            int gridX = (int)room.gridCoord.x;
            int gridY = (int)room.gridCoord.y;

            if (gridX < lowestX)
                lowestX = gridX;
            if (gridY < lowestY)
                lowestY = gridY;
        }

        // Return original set if no adjustment is needed
        if (lowestX >= 0 && lowestY >= 0)
            return rooms;

        // Adjust coordinates
        foreach (Room room in rooms)
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
            normalizedRooms.Add(normalizedRoom);
        }


        return normalizedRooms;
    }



    #region DEBUG
    private void DebugRoom(Room room)
    {
        foreach (Vector2 site in room.siteCoords)
        {
            Instantiate(debugCube, new Vector3(site.x, 0, site.y), Quaternion.identity);
        }

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
        GameObject marker = Instantiate(missionMarker, new Vector3(roomOrigin.x, 0, roomOrigin.y), Quaternion.identity);
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
        foreach (List<Vector2> piece in layout)
        {
            for (int i = 0; i < piece.Count; i++)
            {
                Vector2 left;
                Vector2 right;
                if (i + 1 < piece.Count)
                {
                    left = piece[i];
                    right = piece[i + 1];
                }
                else
                {
                    left = piece[i];
                    right = piece[0];
                }
                Gizmos.DrawLine(left, right);
            }
        }
    }

    private void DrawRandomVoronoiCell(string seed)
    {
        Vector2 coord = RandomUtil.RandomElement(voronoi.SiteCoords(), false, seed);
        List<LineSegment> cell = voronoi.VoronoiBoundaryForSite(coord);
        DrawLineSegments(cell, Color.blue);
    }

    private void DrawVoronoi()
    {
        List<LineSegment> diagram = voronoi.VoronoiDiagram();
        DrawLineSegments(diagram, Color.white);
    }
    #endregion
}
