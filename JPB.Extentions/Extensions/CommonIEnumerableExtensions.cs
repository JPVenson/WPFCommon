using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace JPB.Extensions.Extensions
{
    public static class CommonIEnumerableExtensions
    {
        /// <summary>
        ///     Looks for an specific element in source and return his null based position
        /// </summary>
        /// <typeparam name="T">
        ///     Type of <see>Source</see> and <see>element</see>>
        /// </typeparam>
        /// <param name="source">An IEnumerable</param>
        /// <param name="element">The wanted element</param>
        /// <returns>
        ///     The pos of <see>element</see>"/>
        /// </returns>
        public static int IndexOf<T>(this IEnumerable<T> source, T element)
        {
            for (var i = 0; i < source.Count(); i++)
            {
                if (element.Equals(source.ElementAt(i)))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}