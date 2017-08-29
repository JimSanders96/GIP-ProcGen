using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointGenerator : MonoBehaviour {

    public string seed = "lolwut";
    public int amount, minX, maxX, minZ, maxZ;
    public Vector3 pointDrawSize = new Vector3(.25f, .25f, .25f);

    private System.Random random;

    private void Awake()
    {
    }

    private void OnDrawGizmos()
    {
        DrawPoints(GenerateRandomPoints(amount, minX, maxX, minZ, maxZ), pointDrawSize);
    }

    private List<Vector3> GenerateRandomPoints(int amount, int minX, int maxX, int minZ, int maxZ)
    {
        List<Vector3> points = new List<Vector3>();

        random = new System.Random(seed.GetHashCode());

        for (int i = 0; i < amount; i++)
        {
            int x = random.Next(minX, maxX);
            int z = random.Next(minZ, maxZ);
            Vector3 p = new Vector3(x,0, z);
            points.Add(p);
        }

        return points;
    }

    private void DrawPoints(List<Vector3> points, Vector3 size)
    {
        foreach(Vector3 point in points)
        {
            Gizmos.DrawCube(point, size);
        }
    }
}
