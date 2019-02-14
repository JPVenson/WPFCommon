using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

namespace JPB.Extentions.Extensions
{
    /// <summary>
    ///     Defines a list of common used extensions for IEnumerables
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Removes all items that matches the predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// predicate
        /// </exception>
        [PublicAPI]
        public static IEnumerable<T> RemoveWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            var enumerable = source as IList<T> ?? source.ToList();
            var elements = enumerable.Where(predicate).ToArray();

            foreach (var item in elements)
            {
                enumerable.Remove(item);
            }

            return enumerable;
        }

        /// <summary>
        ///     Enumerates all items and Concatenates the value of getProperty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="getProperty">The get property.</param>
        /// <returns></returns>
        public static string ToPropertyCsv<T>(this IEnumerable<T> source, Func<T, string> getProperty)
        {
            return source.Any() ? source.Select(getProperty).Aggregate((s, e) => s + "," + e) : string.Empty;
        }
    }
}