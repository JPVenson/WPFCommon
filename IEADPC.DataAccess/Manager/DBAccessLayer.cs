using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using IEADPC.DataAccess.AdoWrapper;
using IEADPC.DataAccess.ModelsAnotations;

namespace IEADPC.DataAccess.Manager
{
    public class DbAccessLayer
    {
        public IDatabase Database { get; set; }

        public void Update<T>(T entry) where T : new()
        {
            Update(entry, Database);
        }

        public void Insert<T>(T entry)
        {
            Insert(entry, Database);
        }

        public T InsertWithSelect<T>(T entry) where T : new()
        {
            return InsertWithSelect(entry, Database);
        }

        public void InsertRange<T>(IEnumerable<T> entry)
        {
            Database.RunInTransaction(s =>
            {
                foreach (T item in entry)
                    Insert(item, s);
            });
        }

        public void Delete<T>(T entry)
        {
            Delete<T, long>(entry, Database);
        }

        public void Delete<T, E>(T entry)
        {
            Delete<T, E>(entry, Database);
        }

        public int ExecuteGenericCommand(string query, IEnumerable<IQueryParameter> values)
        {
            var command = CreateCommand(Database, query);

            foreach (var item in values)
            {
                command.Parameters.AddWithValue(item.Name, item.Value);
            }

            return Database.Run(s => s.ExecuteNonQuery(command));
        }

        public int ExecuteGenericCommand(string query, dynamic paramenter)
        {
            return ExecuteGenericCommand(query, (IEnumerable<IQueryParameter>)EnumarateFromDynamics(paramenter));
        }

        public int ExecuteGenericCommand(SqlCommand command)
        {
            return Database.Run(s => s.ExecuteNonQuery(command));
        }

        public static int ExecuteGenericCommand(SqlCommand command, IDatabase batchRemotingDb)
        {
            return batchRemotingDb.Run(s => s.ExecuteNonQuery(command));
        }

        public static List<T> ExecuteGenericCreateModelsCommand<T>(SqlCommand command, IDatabase batchRemotingDb)
            where T : new()
        {
            return batchRemotingDb.Run(
                s =>
                s.GetEntitiesList(command, e => new T().SetPropertysViaRefection(e))
                 .ToList());
        }

        public static void Delete<T>(T entry, IDatabase batchRemotingDb)
        {
            Type type = typeof(T);
            string proppk = type.GetPK();
            string query = "DELETE FROM " + type.GetTableName() + " WHERE " + proppk + " = @pk";

            batchRemotingDb.Run(s =>
            {
                SqlCommand cmd = CreateCommand(s, query);
                cmd.Parameters.AddWithValue("@pk", entry.GetPK<T, long>());
                s.ExecuteNonQuery(cmd);
            });
        }

        public static void Delete<T, E>(T entry, IDatabase batchRemotingDb)
        {
            Type type = typeof(T);
            string proppk = type.GetPK();
            string query = "DELETE FROM " + type.GetTableName() + " WHERE " + proppk + " = @pk";

            batchRemotingDb.Run(s =>
            {
                SqlCommand cmd = CreateCommand(s, query);
                cmd.Parameters.AddWithValue("@pk", entry.GetPK<T, E>());
                s.ExecuteNonQuery(cmd);
            });
        }

        public T Refresh<T>(T entry) where T : new()
        {
            return Select<T>(entry.GetPK<T, long>(), Database);
        }

        public static SqlCommand CreateSelect(Type type, long pk, IDatabase batchRemotingDb)
        {
            throw new NotImplementedException();
            //string proppk = type.GetPK();
            //string propertyInfos = CreatePropertyCSV<>();
            //string query = "SELECT " + propertyInfos + " FROM " + type.GetTableName() + " WHERE " + proppk + " = @pk";
            //var cmd = CreateCommand(batchRemotingDb, query);
            //cmd.Parameters.AddWithValue("@pk", pk);
            //return cmd;
        }


        public static SqlCommand CreateSelect<T>(IDatabase batchRemotingDb, string query,
                                                 params Tuple<string, object>[] paramenter)
        {
            var plainCommand = CreateCommand(batchRemotingDb,
                                             CreateSelect<T>(batchRemotingDb).CommandText + " " + query);
            foreach (var para in paramenter)
                plainCommand.Parameters.AddWithValue(para.Item1, para.Item2);
            return plainCommand;
        }

        public static SqlCommand CreateSelect<T>(IDatabase batchRemotingDb, string query,
                                          IEnumerable<IQueryParameter> paramenter)
        {
            var plainCommand = CreateCommand(batchRemotingDb,
                                             CreateSelect<T>(batchRemotingDb).CommandText + " " + query);
            foreach (var para in paramenter)
                plainCommand.Parameters.AddWithValue(para.Name, para.Value);
            return plainCommand;
        }

        public static SqlCommand CreateSelect<T>(long pk, IDatabase batchRemotingDb) where T : new()
        {
            Type type = typeof(T);
            string proppk = type.GetPK();
            string propertyInfos = CreatePropertyCSV<T>();
            string query = "SELECT " + propertyInfos + " FROM " + type.GetTableName() + " WHERE " + proppk + " = @pk";
            var cmd = CreateCommand(batchRemotingDb, query);
            cmd.Parameters.AddWithValue("@pk", pk);
            return cmd;
        }

        public static IDbCommand CreateUpdate<T>(T entry, IDatabase batchRemotingDb) where T : new()
        {
            Type type = typeof(T);
            string pk = type.GetPK();

            string[] ignore =
                type.GetProperties()
                    .Where(s => s.CheckForPK() || s.GetCustomAttributes(false).Any(e => e is InsertIgnore))
                    .Select(s => s.Name)
                    .ToArray();

            string[] propertyInfos = CreatePropertyNames<T>(ignore).ToArray();

            string prop = " SET ";
            for (int index = 0; index < propertyInfos.Length; index++)
            {
                string info = propertyInfos[index];
                prop = prop + (info + " = @" + index + ",");
            }

            prop = prop.Remove(prop.Length - 1);

            string query = "UPDATE " + type.GetTableName() + prop + " WHERE " + pk + " = " + entry.GetPK();

            return CreateCommandWithParameterValues(query, propertyInfos, entry, batchRemotingDb);
        }

        public static object GetDataValue(object value)
        {
            return value ?? DBNull.Value;
        }

        protected static SqlCommand CreateCommand(IDatabase batchRemotingDb, string query)
        {
            var conn = batchRemotingDb.GetConnection() as SqlConnection;
            var trans = batchRemotingDb.GetTransaction() as SqlTransaction;
            SqlCommand cmd;
            if (trans != null)
                cmd = new SqlCommand(query, conn, trans);
            else
                cmd = new SqlCommand(query, conn);
            return cmd;
        }

        public static IDbCommand CreateCommandWithParameterValues<T>(string query, string[] propertyInfos, T entry,
                                                                        IDatabase batchRemotingDb)
        {
            object[] propertyvalues =
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
                cmd.Parameters.AddWithValue("@" + index, propertyInfo);
            }
            return cmd;
        }

        public static IDbCommand CreateInsert<T>(T entry, IDatabase batchRemotingDb)
        {
            Type type = typeof(T);
            string[] ignore =
                type.GetProperties()
                    .Where(s => s.CheckForPK() || s.GetCustomAttributes(false).Any(e => e is InsertIgnore))
                    .Select(s => s.Name)
                    .ToArray();
            string[] propertyInfos = CreatePropertyNames<T>(ignore).ToArray();
            string csvprops = CreatePropertyCSV<T>(ignore);

            string values = "";
            for (int index = 0; index < propertyInfos.Length; index++)
                values = values + ("@" + index + ",");
            values = values.Remove(values.Length - 1);
            string query = "INSERT INTO " + type.GetTableName() + " ( " + csvprops + " ) VALUES ( " + values + " )";

            var orignialProps = type.GetPropertysViaRefection(ignore).ToArray();

            return CreateCommandWithParameterValues(query, orignialProps, entry, batchRemotingDb);
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb)
        {
            string query = "SELECT " + CreatePropertyCSV<T>() + " FROM " + typeof(T).GetTableName();
            IDbCommand cmd = CreateCommand(batchRemotingDb, query);
            return cmd;
        }

        public T Select<T>(long pk) where T : new()
        {
            return Select<T>(pk, Database);
        }

        public List<T> Select<T>() where T : new()
        {
            return Select<T>(Database);
        }

        public static List<T> RunSelect<T>(IDatabase database, SqlCommand query) where T : new()
        {
            return
                database.Run(
                    s =>
                        s.GetEntitiesList(query, e => new T().SetPropertysViaRefection(e)).ToList());
        }

        public static List<T> RunSelect<T>(IDatabase database, string query, IEnumerable<IQueryParameter> paramenter) where T : new()
        {

            return
                database.Run(
                    s =>
                    {
                        var command = CreateCommand(s, query);

                        foreach (var item in paramenter)
                        {
                            command.Parameters.AddWithValue(item.Name, item.Value);
                        }
                        return s.GetEntitiesList(command, e => new T().SetPropertysViaRefection(e)).ToList();
                    }
                    );
        }

        private List<T> RunSelect<T>(SqlCommand command) where T : new()
        {
            return RunSelect<T>(Database, command);
        }

        private List<T> RunNativeSelect<T>(string query) where T : class
        {
            return
                Database.Run(
                s =>
                s.GetEntitiesList(CreateCommand(s, query), e => e[0] as T)
            .ToList());
        }

        private List<T> RunSelect<T>(string query) where T : new()
        {
            return
                Database.Run(
                s =>
                s.GetEntitiesList(CreateCommand(s, query), e => new T().SetPropertysViaRefection(e))
            .ToList());
        }

        public List<T> SelectNative<T>(string query) where T : class
        {
            return RunNativeSelect<T>(query);
        }

        public List<T> Select<T>(string query) where T : new()
        {
            return RunSelect<T>(query);
        }

        public interface IQueryParameter
        {
            string Name { get; set; }
            object Value { get; set; }
        }

        public class QueryParameter : IQueryParameter
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        public List<T> Select<T>(string query, IEnumerable<IQueryParameter> paramenter) where T : new()
        {
            return RunSelect<T>(Database, query, paramenter);
        }

        public List<T> Select<T>(string query, dynamic paramenter) where T : new()
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = EnumarateFromDynamics(paramenter);
            return Select<T>(query, enumarateFromDynamics);
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

        public List<T> SelectWhere<T>(String @where) where T : new()
        {
            var query = CreateSelect<T>(Database, @where);
            return RunSelect<T>(query);
        }

        public List<T> SelectWhere<T>(String @where, long top) where T : new()
        {
            string query = CreateSelect<T>(Database).CommandText.Insert(6, " TOP " + top + " ") + " " + @where;
            return RunSelect<T>(query);
        }

        public List<T> SelectWhere<T>(String @where, params Tuple<string, object>[] paramenter) where T : new()
        {
            var query = CreateSelect<T>(Database, @where, paramenter);
            return RunSelect<T>(query);
        }

        public List<T> SelectWhere<T>(String @where, dynamic paramenter) where T : new()
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = EnumarateFromDynamics(paramenter);
            var query = CreateSelect<T>(Database, @where, enumarateFromDynamics);
            return RunSelect<T>(query);
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

        protected static List<T> Select<T>(IDatabase batchRemotingDb) where T : new()
        {
            var list = RunSelect<T>(batchRemotingDb, CreateSelect<T>(batchRemotingDb) as SqlCommand).ToList();

            foreach (var model in list)
                model.LoadNavigationProps(batchRemotingDb);

            return list;
        }

        protected static T Select<T>(long pk, IDatabase batchRemotingDb) where T : new()
        {
            return
                batchRemotingDb.Run(
                    s =>
                    s.GetEntitiesList(CreateSelect<T>(pk, batchRemotingDb), e => new T().SetPropertysViaRefection(e))
                     .FirstOrDefault()).LoadNavigationProps(batchRemotingDb);
        }

        public static void Update<T>(T entry, IDatabase db) where T : new()
        {
            db.Run(s => { s.ExecuteNonQuery(CreateUpdate(entry, s)); });
        }

        public static void Insert<T>(T entry, IDatabase db)
        {
            db.Run(s => { s.ExecuteNonQuery(CreateInsert(entry, s)); });
        }

        public static T InsertWithSelect<T>(T entry, IDatabase db) where T : new()
        {
            return db.Run(s =>
            {
                var dbCommand = CreateInsert(entry, s);
                dbCommand.CommandText += "; SELECT SCOPE_IDENTITY();";
                object executeScalar = dbCommand.ExecuteScalar();
                return Select<T>(Convert.ToInt64(executeScalar), s);
            });
        }
    }
}