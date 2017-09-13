using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtil
{

    #region Clockwise Comparer

    // Copied from https://pastebin.com/1RkaP28U

    /// <summary>
    ///     ClockwiseComparer provides functionality for sorting a collection of Vector2s such
    ///     that they are ordered clockwise about a given origin.
    /// </summary>
    public class ClockwiseComparer : IComparer<Vector2>
    {
        private Vector2 m_Origin;

        #region Properties

        /// <summary>
        ///     Gets or sets the origin.
        /// </summary>
        /// <value>The origin.</value>
        public Vector2 origin { get { return m_Origin; } set { m_Origin = value; } }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the ClockwiseComparer class.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public ClockwiseComparer(Vector2 origin)
        {
            m_Origin = origin;
        }

        #region IComparer Methods

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        public int Compare(Vector2 first, Vector2 second)
        {
            return IsClockwise(first, second, m_Origin);
        }

        #endregion

        /// <summary>
        ///     Returns 1 if first comes before second in clockwise order.
        ///     Returns -1 if second comes before first.
        ///     Returns 0 if the points are identical.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        /// <param name="origin">Origin.</param>
        public static int IsClockwise(Vector2 first, Vector2 second, Vector2 origin)
        {
            if (first == second)
                return 0;

            Vector2 firstOffset = first - origin;
            Vector2 secondOffset = second - origin;

            float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.y);
            float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.y);

            if (angle1 < angle2)
                return -1;

            if (angle1 > angle2)
                return 1;

            // Check to see which point is closest
            return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? -1 : 1;
        }
    }

    #endregion

    /// <summary>
    /// Returns the average Vector2 of a given set of Vector2's.
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static Vector2 FindOrigin(Vector2[] points)
    {
        if (points.Length == 0)
            return Vector2.zero;
        if (points.Length == 1)
            return points[0];

        Vector2 origin = Vector2.zero;
        for (int i = 0; i < points.Length; i++)
            origin += points[i];

        return origin / points.Length;
    }

    /// <summary>
    /// Returns the average Vector2 of a given set of Vector2's.
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static Vector2 FindOrigin(List<Vector2> points)
    {
        if (points.Count == 0)
            return Vector2.zero;
        if (points.Count == 1)
            return points[0];

        Vector2 origin = Vector2.zero;
        foreach (Vector2 point in points)
        {
            origin += point;
        }

        return origin / points.Count;
    }

    /// <summary>
    /// Sorts the given set of Vector2's clockwise around the average of all vectors.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<Vector2> SortClockwise(List<Vector2> list)
    {
        Vector2[] pointsArray = list.ToArray();
        Vector2 origin = FindOrigin(pointsArray);
        Array.Sort(pointsArray, new ClockwiseComparer(origin));
        List<Vector2> clockwisePoints = new List<Vector2>(pointsArray);
        return clockwisePoints;
    }

    /// <summary>
    /// Sorts the given set of Vector2's clockwise around the average of all vectors.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static Vector2[] SortClockwise(Vector2[] array)
    {
        Vector2[] clockwisePoints = array;
        Vector2 origin = FindOrigin(clockwisePoints);
        Array.Sort(clockwisePoints, new ClockwiseComparer(origin));
        return clockwisePoints;
    }
}
