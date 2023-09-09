using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Util
{
    public static class ListUtil
    {
        /// <summary>
        /// Gets a random entry from a list or array with a provided random number generator.
        /// </summary>
        /// <param name="list">List or array of elements</param>
        /// <param name="random">Random number generator to generate a random index</param>
        /// <typeparam name="T">Type elements in array</typeparam>
        /// <returns>Random element</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if zero length list/array is provided.</exception>
        public static T GetRandom<T>(this IReadOnlyList<T> list, Random random)
        {
            if (list.Count == 0) Debug.LogError("[ListUtil::GetRandom] empty list provided.");
            int index = random.Next(0, list.Count);
            return list[index];
        }

        /// <summary>
        /// Gets a random entry from a list or array with a provided random number generator.
        /// </summary>
        /// <param name="list">List or array of elements</param>
        /// <param name="random">Random number generator to generate a random index</param>
        /// <param name="val">Random element</param>
        /// <typeparam name="T">Type elements in array</typeparam>
        /// <returns>Index of random element</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if zero length list/array is provided.</exception>
        public static int GetRandom<T>(this IReadOnlyList<T> list, Random random, out T val)
        {
            if (list.Count == 0) Debug.LogError("[ListUtil::GetRandom] empty list provided.");
            int index = random.Next(0, list.Count);
            val = list[index];
            return index;
        }

        /// <summary>
        /// Removes an element from the list by swapping it with the last element and removing it.
        ///
        /// Used for removing from a list of random elements.
        /// </summary>
        /// <param name="list">List (of random elements) to remove from</param>
        /// <param name="index">Index to remove</param>
        /// <typeparam name="T">Type of elements in list</typeparam>
        public static T SwapRemove<T>(this List<T> list, int index)
        {
            int last = list.Count - 1;
            T val = list[index];
            list[index] = list[last];
            list.RemoveAt(last);
            return val;
        }
    }
}