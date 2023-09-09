using System;
using System.Collections.Generic;
using System.Linq;

namespace Util
{
    /// <summary>
    /// Utility class to break apart flags
    /// </summary>
    public static class FlagUtil
    {
        /// <summary>
        /// Get all parts of a flag enum.
        /// </summary>
        /// <param name="val">Enum flag to break into parts.</param>
        /// <typeparam name="T">Type of enum (for casting).</typeparam>
        /// <returns>List of all parts.</returns>
        public static List<T> GetParts<T>(this T val) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Where(t => val.HasFlag(t)).ToList();
        }
    }
}