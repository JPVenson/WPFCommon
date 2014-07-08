using System;
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
            var command = CreateCommand(Database, query);

            foreach (var item in values)
            {
                command.Parameters.AddWithValue(item.Name, item.Value, this.Database);
            }

            return Database.Run(s => s.ExecuteNonQuery(command));
        }

        public int ExecuteGenericCommand(string query, dynamic paramenter)
        {
            return ExecuteGenericCommand(query, (IEnumerable<IQueryParameter>)EnumarateFromDynamics(paramenter));
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
            //var conn = batchRemotingDb.GetConnection();
            //var trans = batchRemotingDb.GetTransaction();
            return batchRemotingDb.CreateCommand(query);

            //SqlCommand cmd;
            //if (trans != null)
            //    cmd = new SqlCommand(query, conn, trans);
            //else
            //    cmd = new SqlCommand(query, conn);
            //return conn;
        }

        public static IDbCommand CreateCommandWithParameterValues<T>(string query, string[] propertyInfos, T entry,
                                                                        IDatabase batchRemotingDb)
        {
            var propertyvalues =
                propertyInfos.Select(
                    propertyInfo => GetDataValue(typeof(T).GetProperty(propertyInfo).GetValue(entry, null))).ToArray();
            return CreateCommandWithParameterValues(query, batchRemotingDb, propertyvalues);
        }

        public static IDbCommand CreateCommandWithParameterValues(string query, IDatabase batchRemotingDb,
                                                                     object[] values)
        {
            var cmd = CreateCommand(batchRemotingDb, query);
            for (int index = 0; index < values.Length; index++)
            {
                object propertyInfo = values[index];
                var dbDataParameter = cmd.CreateParameter();
                dbDataParameter.Value = propertyInfo;
                dbDataParameter.ParameterName = "@" + index;
                cmd.Parameters.Add(dbDataParameter);
            }
            return cmd;
        }

        private static IEnumerable<IQueryParameter> EnumarateFromDynamics(dynamic parameter)
        {
            var list = new List<IQueryParameter>();

            var propertys = ((Type)parameter.GetType()).GetProperties();

            for (int i = 0; i < propertys.Length; i++)
            {
                PropertyInfo element = propertys[i];
                var value = DataConverterExtensions.GetParamaterValue(parameter, element.Name);
                list.Add(new QueryParameter() { Name = "@" + element.Name, Value = value });
            }

            return list;
        }

        protected static string CreatePropertyCSV<T>(bool ignorePK = false)
        {
            return CreatePropertyNames<T>(ignorePK).Aggregate((e, f) => e + ", " + f);
        }

        protected static string CreatePropertyCSV<T>(params string[] ignore)
        {
            return CreatePropertyNames<T>(ignore).Aggregate((e, f) => e + ", " + f);
        }

        protected static IEnumerable<string> CreatePropertyNames<T>(params string[] ignore)
        {
            List<string> propnames = DataConverterExtensions.MapEntiyToSchema<T>(ignore).ToList();
            return propnames;
        }

        protected static IEnumerable<string> CreatePropertyNames<T>(bool ignorePK = false)
        {
            return ignorePK ? CreatePropertyNames<T>(typeof(T).GetPK()) : CreatePropertyNames<T>(new string[0]);
        }
    }
}