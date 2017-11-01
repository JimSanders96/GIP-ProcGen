using System.Collections;
using System.Collections.Generic;
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

    private List<Room> rooms;

    private void Awake()
    {
        rooms = new List<Room>();
        StartCoroutine(Test());
    }


    private IEnumerator Test()
    {
        Debug.Log("Starting test routine");
        while (rooms.Count < 5)
        {
            Vector2 coord = rooms.Count == 0 ? Vector2.zero : GetAvailableNeighborGridCoord(rooms[rooms.Count - 1].gridCoord);
            Room room = CreateRoom(coord, 5);
            DebugRoom(room);
            yield return new WaitForSeconds(1);
        }
    }

    /// <summary>
    /// Generate a layout based on a mission graph.
    /// </summary>
    /// <param name="mission"></param>
    /// <returns></returns>
    public List<List<Vector2>> GenerateLayout(Graph<MissionNodeData> mission)
    {
        // Init vars
        List<List<Vector2>> layout = new List<List<Vector2>>();
        rooms = new List<Room>();

        return layout;
    }

    /// <summary>
    /// Returns a list of points, representing the voronoi sites of a room.
    /// Automatically puts it in the rooms list.
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

        // Keep track of created rooms
        rooms.Add(room);

        Debug.Log("Created room at grid tile " + room.gridCoord);
        return room;
    }

    private Vector2 GetAvailableNeighborGridCoord(Vector2 startCoord)
    {
        // Get all surrounding coords (filter out used coords)
        List<Vector2> availableCoords = new List<Vector2>();
        for (int x = (int)startCoord.x - 1; x < (int)startCoord.x + 1; x++)
        {
            for (int y = (int)startCoord.y - 1; y < (int)startCoord.y + 1; y++)
            {
                Vector2 coord = new Vector2(x, y);
                if (coord != startCoord && !GridCoordIsTaken(coord))
                    availableCoords.Add(coord);
            }
        }

        // Shuffle list & return first entry
        System.Random random = new System.Random(System.DateTime.Now.GetHashCode());
        random.Shuffle(availableCoords);
        return availableCoords[0];
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
    /// Returns true if the given gridCoord exists as the gridCoord of one of the rooms.
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <returns></returns>
    private bool GridCoordIsTaken(Vector2 gridCoord)
    {
        foreach (Room room in rooms)
            if (room.gridCoord == gridCoord)
                return true;
        return false;
    }

    /// <summary>
    /// Places a mission marker at the site location, displaying the data provided.
    /// </summary>
    /// <param name="site"></param>
    /// <param name="data"></param>
    private void PlaceMissionMarker(Vector2 site, MissionNodeData data)
    {
        GameObject marker = Instantiate(missionMarker, site, Quaternion.identity);
        marker.GetComponent<MissionMarker>().Init(data);
    }

    private void DebugRoom(Room room)
    {
        foreach (Vector2 site in room.siteCoords)
        {
            Instantiate(debugCube, new Vector3(site.x, 0, site.y), Quaternion.identity);
        }
    }
}
