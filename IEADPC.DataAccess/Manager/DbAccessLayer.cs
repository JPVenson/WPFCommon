﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using DataAccess.AdoWrapper;
using DataAccess.Helper;

namespace DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        public IDatabase Database { get; set; }

        public bool CheckDatabase()
        {
            if (Database == null)
                return false;
            Database.Connect(false);
            Database.CloseConnection();
            return true;
        }

        public int ExecuteGenericCommand(string query, IEnumerable<IQueryParameter> values)
        {
            IDbCommand command = CreateCommand(Database, query);

            foreach (IQueryParameter item in values)
                command.Parameters.AddWithValue(item.Name, item.Value, Database);

            return Database.Run(s => s.ExecuteNonQuery(command));
        }

        public int ExecuteGenericCommand(string query, dynamic paramenter)
        {
            return ExecuteGenericCommand(query, (IEnumerable<IQueryParameter>) EnumarateFromDynamics(paramenter));
        }

        public int ExecuteGenericCommand(IDbCommand command)
        {
            return Database.Run(s => s.ExecuteNonQuery(command));
        }

        public static int ExecuteGenericCommand(IDbCommand command, IDatabase batchRemotingDb)
        {
            return batchRemotingDb.Run(s => s.ExecuteNonQuery(command));
        }

        public static List<T> ExecuteGenericCreateModelsCommand<T>(IDbCommand command, IDatabase batchRemotingDb)
            where T : new()
        {
            return batchRemotingDb.Run(
                s =>
                    s.GetEntitiesList(command, e => new T().SetPropertysViaRefection(e))
                        .ToList());
        }

        public static object GetDataValue(object value)
        {
            return value ?? DBNull.Value;
        }

        protected static IDbCommand CreateCommand(IDatabase batchRemotingDb, string query)
        {
            return batchRemotingDb.CreateCommand(query);
        }

        public static IDbCommand CreateCommandWithParameterValues<T>(string query, string[] propertyInfos, T entry,
            IDatabase batchRemotingDb)
        {
            Type type = typeof (T);
            object[] propertyvalues =
                propertyInfos.Select(
                    propertyInfo =>
                    {
                        PropertyInfo property =
                            type.GetProperty(DataConverterExtensions.ReMapSchemaToEntiysProp<T>(propertyInfo));
                        object dataValue = GetDataValue(property.GetValue(entry, null));
                        return dataValue;
                    }).ToArray();
            return CreateCommandWithParameterValues(query, batchRemotingDb, propertyvalues);
        }

        public static IDbCommand CreateCommandWithParameterValues(string query, IDatabase batchRemotingDb,
            object[] values)
        {
            var listofQueryParamter = new List<IQueryParameter>();
            for (int i = 0; i < values.Count(); i++)
                listofQueryParamter.Add(new QueryParameter {Name = i.ToString(), Value = values[i]});
            return CreateCommandWithParameterValues(query, batchRemotingDb, listofQueryParamter);
        }

        public static IDbCommand CreateCommandWithParameterValues(string query, IDatabase batchRemotingDb,
            IEnumerable<IQueryParameter> values)
        {
            IDbCommand cmd = CreateCommand(batchRemotingDb, query);
            foreach (IQueryParameter queryParameter in values)
            {
                IDbDataParameter dbDataParameter = cmd.CreateParameter();
                dbDataParameter.Value = queryParameter.Value;
                dbDataParameter.ParameterName = !queryParameter.Name.StartsWith("@")
                    ? "@" + queryParameter.Name
                    : queryParameter.Name;
                cmd.Parameters.Add(dbDataParameter);
            }
            return cmd;
        }

        private static IEnumerable<IQueryParameter> EnumarateFromDynamics(dynamic parameter)
        {
            var list = new List<IQueryParameter>();

            PropertyInfo[] propertys = ((Type) parameter.GetType()).GetProperties();

            for (int i = 0; i < propertys.Length; i++)
            {
                PropertyInfo element = propertys[i];
                dynamic value = DataConverterExtensions.GetParamaterValue(parameter, element.Name);
                list.Add(new QueryParameter {Name = "@" + element.Name, Value = value});
            }

            return list;
        }

        protected static string CreatePropertyCSV(Type type, bool ignorePK = false)
        {
            return CreatePropertyNames(type, ignorePK).Aggregate((e, f) => e + ", " + f);
        }

        protected static string CreatePropertyCSV<T>(bool ignorePK = false)
        {
            return CreatePropertyCSV(typeof (T), ignorePK);
        }

        protected static string CreatePropertyCSV(Type type, params string[] ignore)
        {
            IEnumerable<string> propertyNames = CreatePropertyNames(type, ignore);
            return propertyNames.Aggregate((e, f) => e + ", " + f);
        }

        protected static string CreatePropertyCSV<T>(params string[] ignore)
        {
            return CreatePropertyCSV(typeof (T), ignore);
        }

        protected static IEnumerable<string> CreatePropertyNames(Type type, params string[] ignore)
        {
            return DataConverterExtensions.MapEntiyToSchema(type, ignore).ToList();
        }

        protected static IEnumerable<string> CreatePropertyNames<T>(params string[] ignore)
        {
            return CreatePropertyNames(typeof (T), ignore);
        }

        protected static IEnumerable<string> CreatePropertyNames(Type type, bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames(type, type.GetPK()) : CreatePropertyNames(type, new string[0]);
        }

        protected static IEnumerable<string> CreatePropertyNames<T>(bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames<T>(typeof (T).GetPK()) : CreatePropertyNames<T>(new string[0]);
        }
    }
}