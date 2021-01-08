using System;
using System.Collections.Generic;
using System.Linq;


namespace JPB.WPFToolsAwesome.Extensions
{
    /// <summary>
    ///     Defines a list of common used extensions for IEnumerables
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Removes all items that matches the predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A list of items excluded</returns>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// predicate
        /// </exception>
    
        public static void RemoveWhere<T>(this IList<T> source, Func<T, bool> predicate)
        {
            if (source == null)
            {
	            throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
	            throw new ArgumentNullException(nameof(predicate));
            }

            var elements = source.Where(predicate).ToArray();

            foreach (var item in elements)
            {
                source.Remove(item);
            }
        }

        /// <summary>
        ///     Enumerates all items and Concatenates the value of getProperty
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string Aggregate(this IEnumerable<string> source, string delimiter)
        {
            return source.Aggregate((s, e) => s + delimiter + e);
        }
    }
}