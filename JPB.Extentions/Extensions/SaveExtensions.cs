using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace JPB.Extentions.Extensions
{
    public static class SaveExtensions
    {
        public static T LoadGenericEx<T>([In, Out] this string Filename) where T : ITypContainer, new()
        {
            if (File.Exists(Filename))
            {
                var ts = new TypeStore();
                ts = LoadFromBinary<TypeStore>(Filename + ".typestore");
                T newc = LoadFromXML<T>(Filename, ts.Typen.ToArray());
                return newc;
            }
            return new T();
        }

        public static void SaveGenericEx<T>(this T Class, string Filename) where T : ITypContainer
        {
            var ts = new TypeStore();
            ts.Typen = Class.GetTyps().ToArray();
            ts.SaveAsBinary(Filename + ".typestoren");
            Class.SaveAsXML(Filename, ts.Typen.ToArray());
        }

        public static T LoadGeneric<T>([In, Out] this string Filename) where T : new()
        {
            if (File.Exists(Filename))
            {
                var ts = new TypeStore();
                ts = LoadFromBinary<TypeStore>(Filename + ".typestore");
                T newc = LoadFromXML<T>(Filename, ts.Typen.ToArray());
                return newc;
            }
            return new T();
        }

        public static void SaveGeneric<T>(this T Class, string Filename)
        {
            var ts = new TypeStore();
            var types = new List<Type>();
            foreach (var propType in Class.GetType().GetProperties().Select(s => s.DeclaringType).ToArray())
            {
                types.AddHelper(propType);
            }

            foreach (var enumerable in types.Where(s => s.GetInterface("IEnumerable") != null))
            {
                var interfaces = enumerable.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    types.AddHelper(@interface);
                }

                if (enumerable.ContainsGenericParameters)
                {
                    foreach (var @generic in enumerable.GetGenericArguments())
                    {
                        types.AddHelper(@generic);
                    }
                }
            }

            ts.Typen = types.ToArray();
            ts.SaveAsBinary(Filename + ".typestoren");
            Class.SaveAsXML(Filename, ts.Typen.ToArray());
        }

        private static void AddHelper(this List<Type> source, Type type)
        {
            if (!source.Contains(type))
            {
                source.Add(type);
            }
        }

        public static T LoadFromXMLString<T>(this string source, params Type[] typs) where T : class
        {
            using (var textReader = new StringReader(source))
            {
                var deserializer = new XmlSerializer(typeof(T), typs);
                return (deserializer.Deserialize(textReader)) as T;
            }
        }

        public static T LoadFromXML<T>(this string FileName) where T : class, new()
        {
            if (File.Exists(FileName))
            {
                using (var textReader = new StreamReader(FileName))
                {
                    var deserializer = new XmlSerializer(typeof(T));
                    return (T)(deserializer.Deserialize(textReader));
                }
            }
            return new T();
        }

        public static void LoadFromXML(this Object A, string FileName, Type[] typs)
        {
            if (File.Exists(FileName))
            {
                using (var textReader = new StreamReader(FileName))
                {
                    var deserializer = new XmlSerializer(A.GetType(), typs);
                    A = (deserializer.Deserialize(textReader));
                }
            }
        }

        public static T LoadFromXML<T>(string FileName, Type[] typs) where T : new()
        {
            if (File.Exists(FileName))
            {
                using (var textReader = new StreamReader(FileName))
                {
                    var deserializer = new XmlSerializer(typeof(T), typs);
                    return (T)(deserializer.Deserialize(textReader));
                }
            }
            return new T();
        }

        public static Stream SaveAsXML(this Object A)
        {
            var serializer = new XmlSerializer(A.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, A);
                stream.Close();
                return stream;
            }
        }

        public static MemoryStream SaveAsXML(this Object A, params Type[] extratyps)
        {
            var serializer = new XmlSerializer(A.GetType(), extratyps);
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, A);
                stream.Close();
                return stream;
            }
        }

        public static void SaveAsXML(this Object A, string fileName)
        {
            var serializer = new XmlSerializer(A.GetType());
            using (var textWriter = new StreamWriter(fileName))
            {
                serializer.Serialize(textWriter, A);
                textWriter.Close();
            }
        }

        public static void SaveAsXML(this Object A, string fileName, params Type[] extratyps)
        {
            var serializer = new XmlSerializer(A.GetType(), extratyps);
            using (var textWriter = new StreamWriter(fileName))
            {
                serializer.Serialize(textWriter, A);
                textWriter.Close();
            }
        }

        public static void SaveAsBinary(this Object A, string FileName)
        {
            using (var fs = new FileStream(FileName, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, A);
            }
        }

        public static byte[] SaveAsBinary(this Object A)
        {
            using (var fs = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, A);
                return fs.ToArray();
            }
        }

        public static A LoadFromBinary<A>(string FileName) where A : new()
        {
            if (File.Exists(FileName))
            {
                using (var fs = new FileStream(FileName, FileMode.Open))
                {
                    var formatter = new BinaryFormatter();
                    return (A)formatter.Deserialize(fs);
                }
            }
            return new A();
        }

        public static object LoadFromBinary(string FileName)
        {
            if (File.Exists(FileName))
            {
                using (var fs = new FileStream(FileName, FileMode.Open))
                {
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(fs);
                }
            }
            return null;
        }

        public static object LoadFromBinary(this byte[] FileName)
        {
            using (var memstream = new MemoryStream(FileName))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(memstream);
            }
        }
    }
}