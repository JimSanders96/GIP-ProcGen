using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGeneratorRevamp : MonoBehaviour
{

    public int gridSize = 50;
    [Range(0.1f, 0.5f)]
    public float gridTileBorderThickness = 0.3f;
    public int gridWidth;
    public int gridHeight;
    public string seed;

    private System.Random random;
    private int[][] grid;
    private Dictionary<Vector2, List<Vector2>> gridToSites; // use grid coordinate to lookup corresponding room sites


    /// <summary>
    /// Return the same random every time this is called
    /// </summary>
    /// <returns></returns>
    private System.Random GetRandom()
    {
        if (random == null)
            random = new System.Random(seed.GetHashCode());
        return random;
    }

    private void Awake()
    {

    }

    private void InitGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x][y] = 0;
            }
        }
    }

    public bool CreateLinkedRoom(Vector2 startGridCoord, int pointCount, out Vector2 gridCoord, out List<Vector2> sites)
    {
        // Get available neighbor coord
        Vector2 coord = Vector2.zero;
        bool success = GetAvailableNeighborGridCoord(startGridCoord, out coord);

        // Fail if no neighboring site found
        if (!success)
        {
            sites = null;
            gridCoord = Vector2.zero;            
        }
        else
        {
            // Create room & assign out vars
            sites = CreateRoom(coord, pointCount);
            gridCoord = coord;
        }
        
        return success;
    }

    public void CreateRandomRoom(int pointCount, out Vector2 gridCoord, out List<Vector2> sites)
    {
        // Get a random tile from the grid
        int x = GetRandom().Next(0, gridWidth - 1);
        int y = GetRandom().Next(0, gridHeight - 1);
        Vector2 coord = new Vector2(x, y);        

        // Create the room & assign out vars
        sites = CreateRoom(coord,pointCount);
        gridCoord = coord;
    }

    /// <summary>
    /// Returns a list of points, representing the voronoi sites of a room.
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <param name="pointCount"></param>
    /// <returns></returns>
    private List<Vector2> CreateRoom(Vector2 gridCoord, int pointCount)
    {
        List<Vector2> points = new List<Vector2>();

        // Calculate point boundaries
        int minX = 0;
        int maxX = 0;
        int minY = 0;
        int maxY = 0;
        GetPointBoundaries(gridCoord, out minX, out maxX, out minY, out maxY);

        // Randomly distribute points across the selected grid tile
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 point = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            points.Add(point);
        }
        return points;
    }

    private void GetPointBoundaries(Vector2 gridCoord, out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = ((int)gridCoord.x * gridSize) + (int)(gridSize * gridTileBorderThickness * 0.5f);
        minY = ((int)gridCoord.y * gridSize) + (int)(gridSize * gridTileBorderThickness * 0.5f);
        maxX = ((int)gridCoord.x * gridSize + 1) - (int)(gridSize * gridTileBorderThickness * 0.5f);
        maxY = ((int)gridCoord.y * gridSize + 1) - (int)(gridSize * gridTileBorderThickness * 0.5f);
    }

    private void MarkGridCoordAsTaken(Vector2 gridCoord)
    {
        grid[(int)gridCoord.x][(int)gridCoord.y] = 1;
    }

    // TODO                     TODO
    private bool GetAvailableNeighborGridCoord(Vector2 gridCoord, out Vector2 neighborCoord)
    {
        bool success = false;

        for (int x = (int)gridCoord.x - 1; x < (int)gridCoord.x + 1; x++)
        {
            // TODO
        }

        neighborCoord = Vector2.zero;
        return success;
    }
}
