using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace JPB.Extentions.Extensions
{
    public static class CollectionExtensions
    {
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

        public static string ToPropertyCsv<T>(this IEnumerable<T> source, Func<T, string> getProperty)
        {
            return source.Any() ? source.Select(getProperty).Aggregate((s, e) => s + "," + e) : string.Empty;
        }

        public static IEnumerable<T> CastWhere<T, TE>(this ICollection<TE> source)
        {
            return source.Where(e => e is T).Cast<T>();
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<Object> enumerableList)
            where T : class
        {
            var obCollection = new ObservableCollection<T>();

            foreach (object item in enumerableList)
                obCollection.Add(item as T);

            return obCollection;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerableList)
            where T : class
        {
            var obCollection = new ObservableCollection<T>();

            foreach (T item in enumerableList)
                obCollection.Add(item);

            return obCollection;
        }
    }
}