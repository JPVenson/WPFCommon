#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 13:02

#endregion

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DataAccess.AdoWrapper;
using DataAccess.Manager;
using DataAccess.ModelsAnotations;

#endregion

namespace DataAccess
{

    public static class DataHelperExtensions
    {
        public static void AddWithValue(this IDataParameterCollection source, string name, object parameter, IDatabase db)
        {
            source.Add(db.CreateParameter(name, parameter));
        }
    }

    public static class DataConverterExtensions
    {
        public static string GetTableName<T>()
        {
            var forModel = typeof(T).GetCustomAttributes(false).FirstOrDefault(s => s is ForModel) as ForModel;
            if (forModel != null)
                return forModel.AlternatingName;
            return typeof(T).Name;
        }

        public static string GetTableName(this Type type)
        {
            var forModel = type.GetCustomAttributes(false).FirstOrDefault(s => s is ForModel) as ForModel;
            if (forModel != null)
                return forModel.AlternatingName;
            return type.Name;
        }

        public static object GetParamaterValue(this object source, string name)
        {
            return GetParamater(source, name).GetValue(source, null);
        }

        //public static string GetParamaterName(this object source)
        //{
        //    return GetParamater(source).Name;
        //}

        public static PropertyInfo GetParamater(this object source, string name)
        {
            return source.GetType().GetProperties().FirstOrDefault(s => s.Name == name);
        }


        public static bool CheckForPK(this PropertyInfo info)
        {
            return info.GetCustomAttributes(false).Any(s => s is PrimaryKeyAttribute) || (info.Name.EndsWith("_ID"));
        }

        public static bool CheckForFK(this PropertyInfo info, string name)
        {
            if (info.Name != name)
                return false;
            return info.GetCustomAttributes(false).Any(s => s is PrimaryKeyAttribute);
        }

        public static string GetPK(this Type type)
        {
            var name = type.GetProperties().FirstOrDefault(CheckForPK);
            return MapEntiysPropToSchema(type, name == null ? null : name.Name);
        }

        public static string GetFK(this Type type, string name)
        {
            name = type.ReMapSchemaToEntiysProp(name);
            var prop = type.GetProperties().FirstOrDefault(info => CheckForFK(info, name));
            return prop == null ? null : prop.Name;
        }

        public static long GetPK<T>(this T source)
        {
            string pk = typeof(T).GetPK();
            return (long)typeof(T).GetProperty(pk).GetValue(source, null);
        }

        public static E GetPK<T, E>(this T source)
        {
            string pk = typeof(T).GetPK();
            return (E)typeof(T).GetProperty(pk).GetValue(source, null);
        }

        public static E GetFK<T, E>(this T source, string name)
        {
            string pk = typeof(T).GetFK(name);
            return (E)typeof(T).GetProperty(pk).GetValue(source, null);
        }

        public static E GetFK<E>(this Type targetType, string name)
        {
            string pk = targetType.GetFK(name);
            var propertyInfo = targetType.GetProperty(pk);
            var value = propertyInfo.GetValue(source, null);
            return (E)value;
        }

        public static IEnumerable<string> MapEntiyToSchema<T>(string[] ignore)
        {
            Type type = typeof(T);
            return from propertyInfo in type.GetProperties().Where(s => !s.GetGetMethod().IsVirtual)
                   where !ignore.Contains(propertyInfo.Name)
                   let formodle = propertyInfo.GetCustomAttributes(false).FirstOrDefault(s => s is ForModel) as ForModel
                   select formodle != null ? formodle.AlternatingName : propertyInfo.Name;
        }

        public static string MapEntiysPropToSchema(this Type source, string prop)
        {
            PropertyInfo[] propertys = source.GetProperties();
            return (from propertyInfo in propertys
                    where propertyInfo.Name == prop
                    let formodle =
                        propertyInfo.GetCustomAttributes(false).FirstOrDefault(s => s is ForModel) as ForModel
                    select formodle != null ? formodle.AlternatingName : propertyInfo.Name).FirstOrDefault();
        }

        public static string MapEntiysPropToSchema<T>(string prop)
        {
            return MapEntiysPropToSchema(typeof(T), prop);
        }

        public static string ReMapSchemaToEntiysProp<T>(string prop)
        {
            return typeof (T).ReMapSchemaToEntiysProp(prop);
        }

        public static string ReMapSchemaToEntiysProp(this Type source, string prop)
        {
            foreach (PropertyInfo propertyInfo in from propertyInfo in source.GetProperties()
                                                  let customAttributes =
                                                      propertyInfo.GetCustomAttributes(false)
                                                                  .FirstOrDefault(s => s is ForModel) as ForModel
                                                  where
                                                      customAttributes != null &&
                                                      customAttributes.AlternatingName == prop
                                                  select propertyInfo)
                return propertyInfo.Name;
            return prop;
        }

        public static bool CheckForListInterface(this PropertyInfo info)
        {
            return info.PropertyType != typeof (string)
                   && info.PropertyType.GetInterface(typeof (IEnumerable).Name) != null
                   && info.PropertyType.GetInterface(typeof (IEnumerable<>).Name) != null;
        }

        public static bool CheckForListInterface(this object info)
        {
            return CheckForListInterface(info.GetType());
        }

        public static T LoadNavigationProps<T>(this T source, IDatabase accessLayer)
        {
            return (T)LoadNavigationProps(source as object,accessLayer);
        }

        public static object LoadNavigationProps(this object source, IDatabase accessLayer)
        {
            var virtualProps =
                source.GetType().GetProperties().Where(s => s.GetGetMethod(false).IsVirtual);
            foreach (var propertyInfo in virtualProps)
            {
                var firstOrDefault =
                    propertyInfo.GetCustomAttributes(false).FirstOrDefault(s => s is ForeignKeyAttribute) as
                        ForeignKeyAttribute;
                if (firstOrDefault == null)
                    continue;

                var sqlCommand =
                    DbAccessLayer.CreateSelect<dynamic>(propertyInfo.PropertyType.GetFK<long>(firstOrDefault.AlternatingName),
                        accessLayer);
                var orDefault = DbAccessLayer.RunSelect<dynamic>(accessLayer, sqlCommand);
                propertyInfo.SetValue(source, orDefault, null);

                if (CheckForListInterface(orDefault))
                {
                    var listOfObj = orDefault as IEnumerable;
                    foreach (var item in listOfObj)
                    {
                        item.LoadNavigationProps(accessLayer);
                    }
                }
                else
                {
                    ((object) orDefault).LoadNavigationProps(accessLayer);
                }
            }

            return source;
        }

        public static T SetPropertysViaRefection<T>(this T source, IDataRecord reader)
            where T : new()
        {
            var listofpropertys = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
                listofpropertys.Add(ReMapSchemaToEntiysProp<T>(reader.GetName(i)), reader.GetValue(i));

            foreach (var item in listofpropertys)
            {
                var property = typeof(T).GetProperty(item.Key);
                if (property != null)
                {
                    if (item.Value is DBNull)
                        property.SetValue(source, null, null);
                    else
                    {
                        if (property.PropertyType.IsGenericTypeDefinition && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var convertedValue = System.Convert.ChangeType(item.Value,
                                                                           Nullable.GetUnderlyingType(
                                                                               property.PropertyType));
                            property.SetValue(source, convertedValue, null);
                        }
                        else
                        {
                            property.SetValue(source, item.Value, null);
                        }
                    }
                }
            }

            return source;
        }

        public static Func<object, object> GetGetter(FieldInfo fieldInfo)
        {
            // create a method without a name, object as result type and one parameter of type object
            // the last parameter is very import for accessing private fields
            var method = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(object) }, fieldInfo.Module,
                                           true);
            var il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0); // load the first argument onto the stack (source of type object)
            il.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            // cast the parameter of type object to the type containing the field
            il.Emit(OpCodes.Ldfld, fieldInfo);
            // store the value of the given field on the stack. The casted version of source is used as instance

            if (fieldInfo.FieldType.IsValueType)
                il.Emit(OpCodes.Box, fieldInfo.FieldType);
            // box the value type, so you will have an object on the stack

            il.Emit(OpCodes.Ret); // return the value on the stack

            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        public static Action<object, object> GetSetter(FieldInfo fieldInfo)
        {
            var method = new DynamicMethod(string.Empty, null, new[] { typeof(object), typeof(object) },
                                           fieldInfo.Module, true);
            var il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0); // load the first argument onto the stack (source of type object)
            il.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            // cast the parameter of type object to the type containing the field
            il.Emit(OpCodes.Ldarg_1); // push the second argument onto the stack (this is the value)

            if (fieldInfo.FieldType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType); // unbox the value parameter to the value-type
            else
                il.Emit(OpCodes.Castclass, fieldInfo.FieldType); // cast the value on the stack to the field type

            il.Emit(OpCodes.Stfld, fieldInfo); // store the value on stack in the field
            il.Emit(OpCodes.Ret); // return

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        public static IEnumerable<string> GetPropertysViaRefection(this Type t, params string[] ignore)
        {
            return t.GetProperties().Select(s => s.Name).Except(ignore);
        }
    }
}