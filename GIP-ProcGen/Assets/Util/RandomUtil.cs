using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomUtil
{
    /// <summary>
    /// Returns a random element from the array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T RandomElement<T>(T[] array)
    {
        T element = array[Random.Range(0, array.Length)];
        return element;
    }

    /// <summary>
    /// Returns a random element from the list.
    /// Removes the element from the list when specified.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="removeElement"></param>
    /// <returns></returns>
    public static T RandomElement<T>(List<T> list, bool removeElement, string seed)
    {
        T element;
        if (seed == null)
            element = list[Random.Range(0, list.Count)];
        else
        {
            System.Random r = new System.Random(seed.GetHashCode());
            element = list[r.Next(list.Count)];
        }

        if (removeElement)
            list.Remove(element);

        return element;
    }

    /// <summary>
    /// Returns a random element from the list.
    /// Removes the element from the list when specified.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="removeElement"></param>
    /// <returns></returns>
    public static T RandomElement<T>(List<T> list, bool removeElement)
    {
        T element = list[Random.Range(0, list.Count)];

        if (removeElement)
            list.Remove(element);

        return element;
    }

    /// <summary>
    /// Returns a random integer between min [inclusive] and max [exlusive].
    /// The preferred int has at least preferenceChancePercent chance of being returned.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="preferred"></param>
    /// <param name="preferenceChance"></param>
    /// <returns></returns>
    public static int RangeWithPreference(int min, int max, int preferred, float preferenceChancePercent)
    {
        float f = Random.Range(0f, 1f);

        if (f <= preferenceChancePercent)
            return preferred;

        return Random.Range(min, max);
    }

    /// <summary>
    /// Returns a random integer between min [inclusive] and max [exlusive].
    /// The preferred int has at least preferenceChancePercent chance of being returned.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="preferred"></param>
    /// <param name="preferenceChance"></param>
    /// <returns></returns>
    public static int RangeWithPreference(int min, int max, int preferred, int preferenceChancePercent, string seed)
    {
        System.Random r = new System.Random(seed.GetHashCode());
        float f = r.Next(0, 100);

        if (f <= preferenceChancePercent)
            return preferred;

        return r.Next(min, max);
    }

    /// <summary>
    /// Return a random 2-dimensional integer array filled with 0's and 1's, where fillPercent determines the amount of 1's.
    /// </summary>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <param name="fillPercent"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public static int[,] RandomMap(int maxX, int maxY, int fillPercent, string seed)
    {
        int[,] map = new int[maxX, maxY];

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        // 1 : wall. 0: floor.
        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                map[x, y] = (pseudoRandom.Next(0, 100) < fillPercent) ? 1 : 0;
            }
        }

        return map;
    }

    /// <summary>
    /// System.Random extension. You can call this function from a System.Random instance.
    /// Shuffle an array using the Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rng"></param>
    /// <param name="array"></param>
    public static void Shuffle<T>(this System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    /// <summary>
    /// System.Random extension. You can call this function from a System.Random instance.
    /// Shuffle a list using the Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rng"></param>
    /// <param name="list"></param>
    public static void Shuffle<T>(this System.Random rng, List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
    }

    public static float NextFloat(this System.Random rng)
    {
        double mantissa = (rng.NextDouble() * 2.0) - 1.0;
        double exponent = System.Math.Pow(2.0, rng.Next(-126, 128));
        return (float)(mantissa * exponent);
    }
}
