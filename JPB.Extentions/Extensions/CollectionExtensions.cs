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

        public static string ToPropertyCsv<T, TE>(this IEnumerable<T> source, Func<T, string> getProperty)
        {
            return source.Select(getProperty).Aggregate((s, e) => s + "," + e);
        }

        public static IEnumerable<T> CastWhere<T, TE>(this ICollection<TE> source)
        {
            return source.Where(e => e is T).Cast<T>();
        }

        public static string[] ToStringArray(this List<string> source)
        {
            return source.ToArray();
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

        public static ICollection<T> Cast<T, E>(this ICollection<E> enumerableList, string property)
            where E : class
            where T : class
        {
            PropertyInfo _compareProperty = typeof(E).GetProperty(property);

            //var buff = sourcelist
            //    .FirstOrDefault(s => _compareProperty.GetValue(s, null)
            //        .Equals(_compareProperty.GetValue(Selectetitem, null)));

            var buff = new ObservableCollection<T>();

            foreach (E item in enumerableList)
                buff.Add(_compareProperty.GetValue(item, null));

            return buff;
        }

        public static IEnumerable<T> Cast<T, E>(this IEnumerable<E> enumerableList, string property)
            where E : class
            where T : class
        {
            PropertyInfo _compareProperty = typeof(E).GetProperty(property);

            //var buff = sourcelist
            //    .FirstOrDefault(s => _compareProperty.GetValue(s, null)
            //        .Equals(_compareProperty.GetValue(Selectetitem, null)));

            var buff = new List<T>();

            foreach (E item in enumerableList)
                buff.Add(_compareProperty.GetValue(item, null));

            return buff;
        }
    }
}