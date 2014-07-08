using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataAccess.AdoWrapper;
using DataAccess.Helper;

namespace DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        //public static IDbCommand CreateSelect<T>(Type type,IDatabase batchRemotingDb, long fk)
        //{

        //}

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb, string query)
        {
            var plainCommand = CreateCommand(batchRemotingDb,
                                             CreateSelect<T>(batchRemotingDb).CommandText + " " + query);
            return plainCommand;
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb, string query,
                                          IEnumerable<IQueryParameter> paramenter)
        {
            var plainCommand = CreateCommand(batchRemotingDb,
                                             CreateSelect<T>(batchRemotingDb).CommandText + " " + query);
            foreach (var para in paramenter)
                plainCommand.Parameters.AddWithValue(para.Name, para.Value, batchRemotingDb);
            return plainCommand;
        }

        public static IDbCommand CreateSelect<T>(long pk, IDatabase batchRemotingDb) where T : new()
        {
            Type type = typeof(T);
            string proppk = type.GetPK();
            string propertyInfos = CreatePropertyCSV<T>();
            string query = "SELECT " + propertyInfos + " FROM " + type.GetTableName() + " WHERE " + proppk + " = @pk";
            var cmd = CreateCommand(batchRemotingDb, query);
            cmd.Parameters.AddWithValue("@pk", pk, batchRemotingDb);
            return cmd;
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

        public static List<T> RunSelect<T>(IDatabase database, IDbCommand query) where T : new()
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
                            command.Parameters.AddWithValue(item.Name, item.Value, s);
                        }
                        return s.GetEntitiesList(command, e => new T().SetPropertysViaRefection(e)).ToList();
                    }
                    );
        }

        private List<T> RunSelect<T>(IDbCommand command) where T : new()
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

        public List<T> SelectNative<T>(string query, IEnumerable<IQueryParameter> paramenter) where T : new()
        {
            return RunSelect<T>(Database, query, paramenter);
        }

        public List<T> SelectNative<T>(string query, dynamic paramenter) where T : new()
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = EnumarateFromDynamics(paramenter);
            return SelectNative<T>(query, enumarateFromDynamics);
        }

        public List<T> SelectWhere<T>(String @where) where T : new()
        {
            var query = CreateSelect<T>(Database, @where);
            return RunSelect<T>(query);
        }

        //public List<T> SelectWhere<T>(String @where, long top) where T : new()
        //{
        //    string query = CreateSelect<T>(Database).CommandText.Insert(6, " TOP " + top + " ") + " " + @where;
        //    return RunSelect<T>(query);
        //}

        public List<T> SelectWhere<T>(String @where, IEnumerable<IQueryParameter> paramenter) where T : new()
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

        protected static List<T> Select<T>(IDatabase batchRemotingDb, IDbCommand command) where T : new()
        {
            var list = RunSelect<T>(batchRemotingDb, command).ToList();

            foreach (var model in list)
                model.LoadNavigationProps(batchRemotingDb);

            return list;
        }

        protected static List<T> Select<T>(IDatabase batchRemotingDb) where T : new()
        {
            return Select<T>(batchRemotingDb, CreateSelect<T>(batchRemotingDb) as IDbCommand);
        }

        protected static T Select<T>(long pk, IDatabase batchRemotingDb) where T : new()
        {
            return Select<T>(batchRemotingDb, CreateSelect<T>(pk, batchRemotingDb)).FirstOrDefault();

            //return
            //    batchRemotingDb.Run(
            //        s =>
            //        s.GetEntitiesList(CreateSelect<T>(pk, batchRemotingDb), e => new T().SetPropertysViaRefection(e))
            //         .FirstOrDefault()).LoadNavigationProps(batchRemotingDb);
        }
    }
}
