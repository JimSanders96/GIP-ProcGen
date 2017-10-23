using Delaunay;
using Delaunay.Geo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayoutUtil
{


    /// <summary>
    /// Returns the vertices making up the boundary around the given site.
    /// Vertex set is sorted clockwise for rendering purposes.
    /// </summary>
    /// <param name="voronoi"></param>
    /// <param name="site"></param>
    /// <returns></returns>
    public static List<Vector2> GetVerticesForSite(Voronoi voronoi, Vector2 site)
    {
        List<LineSegment> boundary = voronoi.VoronoiBoundaryForSite(site);
        List<Vector2> vertices = GetVerticesFromLineSegments(boundary);

        List<Vector2> clockwiseVertices = new List<Vector2>(VectorUtil.SortClockwise(vertices));

        return clockwiseVertices;
    }

    /// <summary>
    /// Returns a list of all vertices in the lineSegments. 
    /// Filters out duplicate vertices.
    /// </summary>
    /// <param name="lineSegments"></param>
    /// <returns></returns>
    public static List<Vector2> GetVerticesFromLineSegments(List<LineSegment> lineSegments)
    {
        List<Vector2> vertices = new List<Vector2>();

        // Get vertices from line segments.
        foreach (LineSegment line in lineSegments)
        {
            Vector2 p0 = (Vector2)line.p0;
            Vector2 p1 = (Vector2)line.p1;

            // Filter duplicate vertices
            if (!vertices.Contains(p0))
                vertices.Add(p0);
            if (!vertices.Contains(p1))
                vertices.Add(p1);
        }

        return vertices;
    }

    /// <summary>
    /// Generates a voronoi object based on the width and height parameters of this class.
    /// </summary>
    /// <returns></returns>
    public static Voronoi GenerateVoronoiObject(int pointCount, int width, int height, System.Random random)
    {
        List<Vector2> points = new List<Vector2>();
        List<uint> colors = new List<uint>();

        for (int i = 0; i < pointCount; i++)
        {
            colors.Add(0);
            points.Add(new Vector2(
                    random.Next(0, width),
                    random.Next(0, height))
            );
        }

        return new Voronoi(points, colors, new Rect(0, 0, width, height));
    }

    /// <summary>
    /// Apply Lloyd's relaxation to a voronoi object to 'smoothen' the diagram.
    /// </summary>
    /// <returns></returns>
    public static Voronoi RelaxVoronoi(Voronoi input)
    {
        List<Vector2> siteCoords = input.SiteCoords();
        List<Vector2> adjustedSiteCoords = new List<Vector2>();

        //Reset each site to the centroid of its vertices
        foreach (Vector2 site in siteCoords)
        {
            List<LineSegment> boundary = input.VoronoiBoundaryForSite(site);
            List<Vector2> vertices = GetVerticesFromLineSegments(boundary);
            Vector2 center = VectorUtil.FindOrigin(vertices);
            adjustedSiteCoords.Add(center);
        }

        Voronoi output = new Voronoi(adjustedSiteCoords, input.SiteColors(), input.plotBounds);

        return output;
    }

    /// <summary>
    /// From a set of potential sites, return the one closest to the target site in a given voronoi object.
    /// Excluded sites will not be considered.
    /// </summary>
    /// <param name="voronoi"></param>
    /// <param name="options"></param>
    /// <param name="target"></param>
    /// <param name="excluded"></param>
    /// <returns></returns>
    public static Vector2 GetSiteClosestToTarget(Voronoi voronoi, List<Vector2> options, Vector2 target, List<Vector2> excluded)
    {
        Vector2 closestSite = Vector2.zero;
        float closestDistance = 10000000000f;

        // check the distance to target for each potential site and keep track of the closest one.
        foreach (Vector2 site in options)
        {
            float distance = Vector2.Distance(site, target);
            if (!excluded.Contains(site) && distance < closestDistance)
            {
                closestDistance = distance;
                closestSite = site;
                //Debug.Log("Closest site: " + closestSite);
            }
        }

        return closestSite;
    }

    /// <summary>
    /// Returns all sites on the path between start and target.
    /// Start and target are not included in the path.
    /// Excluded sites will not be checked.
    /// </summary>
    /// <param name="voronoi"></param>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static List<Vector2> GetSitePathToTarget(Voronoi voronoi, Vector2 start, Vector2 target)
    {
        List<Vector2> path = new List<Vector2>();
        Vector2 lastSite = start;
        bool found = false;

        while (!found)
        {
            List<Vector2> neighbors = voronoi.NeighborSitesForSite(lastSite);

            // Check if the target has been reached
            if (neighbors.Contains(target))
            {
                found = true;
                break;
            }

            // Find the neighbor closest to the target and add it to the path
            Vector2 closestSite = GetSiteClosestToTarget(voronoi, neighbors, target, path);
            path.Add(closestSite);
            lastSite = closestSite;
        }

        return path;
    }

    /// <summary>
    /// Returns a list of vertex sets from every site in the site set.
    /// Vertex sets are sorted clockwise.
    /// </summary>
    /// <param name="voronoi"></param>
    /// <param name="startSite"></param>
    /// <param name="targetSite"></param>
    /// <returns></returns>
    public static List<List<Vector2>> GetVerticesForSites(Voronoi voronoi, List<Vector2> sites)
    {
        List<List<Vector2>> vertices = new List<List<Vector2>>();

        // For every site get the site's vertices and add them to the output.
        foreach (Vector2 site in sites)
            vertices.Add(GetVerticesForSite(voronoi, site));

        return vertices;
    }

    /// <summary>
    /// Returns a list of sites surrounding the start site within a square with given width.
    /// Result includes the start site.
    /// </summary>
    /// <param name="voronoi"></param>
    /// <param name="start"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static List<Vector2> GetSitesInSquare(Voronoi voronoi, Vector2 start, float squareWidth)
    {
        List<Vector2> pointsInRadius = new List<Vector2>();

        foreach (Vector2 site in voronoi.SiteCoords())
        {
            if ((site.x >= start.x - squareWidth && site.x <= start.x + squareWidth) && (site.y >= start.y - squareWidth && site.y <= start.y + squareWidth))
                pointsInRadius.Add(site);
        }

        return pointsInRadius;
    }

    public static List<Vector2> GetSitesInRadius(Voronoi voronoi, Vector2 start, float radius)
    {
        List<Vector2> pointsInRadius = new List<Vector2>();

        foreach (Vector2 site in voronoi.SiteCoords())
        {
            if (VectorUtil.WithinRange(start, site, radius))
                pointsInRadius.Add(site);
        }

        return pointsInRadius;
    }

}
