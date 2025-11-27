using System;
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
        /// <returns>Array of all parts.</returns>
        public static T[] GetParts<T>(this T val) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Where(t => val.HasFlag(t)).ToArray();
        }
        
        /// <summary>
        /// Determines if the enum object has multiple parts.
        /// </summary>
        /// <param name="val">Enum flag</param>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <returns>True if there's more than one part, false otherwise</returns>
        public static bool IsSinglePart<T>(this T val) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Count(t => val.HasFlag(t)) == 1;
        }
    }
}