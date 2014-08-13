﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Helper;
using JPB.DataAccess.QueryProvider;

namespace JPB.DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        #region BasicCommands

        public object Select(Type type, long pk)
        {
            return Select(type, pk, Database);
        }

        public T Select<T>(long pk) where T : new()
        {
            return (T)Select(typeof(T), pk);
        }

        protected static object Select(Type type, long pk, IDatabase batchRemotingDb)
        {
            return Select(type, batchRemotingDb).FirstOrDefault();
        }

        protected static T Select<T>(long pk, IDatabase batchRemotingDb) where T : new()
        {
            return Select<T>(batchRemotingDb, CreateSelect<T>(batchRemotingDb, pk)).FirstOrDefault();
        }

        public List<object> Select(Type type)
        {
            return Select(type, Database);
        }

        public List<T> Select<T>() where T : new()
        {
            return Select(typeof(T)).Cast<T>().ToList();
        }

        protected static List<object> Select(Type type, IDatabase batchRemotingDb)
        {
            return Select(type, batchRemotingDb, CreateSelect(type, batchRemotingDb));
        }

        protected static List<T> Select<T>(IDatabase batchRemotingDb) where T : new()
        {
            return Select(typeof(T), batchRemotingDb).Cast<T>().ToList();
        }

        protected static List<object> Select(Type type, IDatabase batchRemotingDb, IDbCommand command)
        {
            return SelectNative(type, batchRemotingDb, command);
        }

        protected static List<T> Select<T>(IDatabase batchRemotingDb, IDbCommand command) where T : new()
        {
            return Select(typeof(T), batchRemotingDb, command).Cast<T>().ToList();
        }

        #endregion

        #region CreateCommands

        public static IDbCommand CreateSelect(Type type, IDatabase batchRemotingDb, string query)
        {
            return CreateCommand(batchRemotingDb, CreateSelect(type, batchRemotingDb).CommandText + " " + query);
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb, string query)
        {
            return CreateSelect(typeof(T), batchRemotingDb, query);
        }

        public static IDbCommand CreateSelect(Type type, IDatabase batchRemotingDb, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand plainCommand = CreateCommand(batchRemotingDb,
                CreateSelect(type, batchRemotingDb).CommandText + " " + query);
            foreach (IQueryParameter para in paramenter)
                plainCommand.Parameters.AddWithValue(para.Name, para.Value, batchRemotingDb);
            return plainCommand;
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            return CreateSelect(typeof(T), batchRemotingDb, query, paramenter);
        }

        public static string[] CreateIgnoreList(Type type)
        {
            return
                type.GetProperties()
                    .Where(
                        s => s.GetGetMethod(false).IsVirtual)
                    .Select(s => s.Name)
                    .ToArray();
        }

        public static IDbCommand CreateSelect(Type type, IDatabase batchRemotingDb, long pk)
        {
            string proppk = type.GetPK();
            string query = CreateSelect(type) + " WHERE " + proppk + " = @pk";
            IDbCommand cmd = CreateCommand(batchRemotingDb, query);
            cmd.Parameters.AddWithValue("@pk", pk, batchRemotingDb);
            return cmd;
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb, long pk) where T : new()
        {
            return CreateSelect(typeof(T), batchRemotingDb, pk);
        }

        public static string CreateSelect<T>()
        {
            return CreateSelect(typeof(T));
        }

        public static string CreateSelect(Type type)
        {
            return "SELECT " + CreatePropertyCSV(type, CreateIgnoreList(type)) + " FROM " + type.GetTableName();
        }

        public static IDbCommand CreateSelect(Type type, IDatabase batchRemotingDb)
        {
            IDbCommand cmd = CreateCommand(batchRemotingDb, CreateSelect(type));
            return cmd;
        }

        public static IDbCommand CreateSelect<T>(IDatabase batchRemotingDb)
        {
            return CreateSelect(typeof(T), batchRemotingDb);
        }

        #endregion

        #region Runs

        public static object RunDynamicSelect(Type type, IDatabase database, IDbCommand query)
        {
            return
                database.Run(
                    s =>
                        s.GetEntitiesList(query, e =>
                        {
                            /*Same as new T()*/
                            dynamic instance = Activator.CreateInstance(type);
                            DataConverterExtensions.SetPropertysViaRefection(instance, e);
                            return instance;
                        }).ToList());
        }

        public static List<object> RunSelect(Type type, IDatabase database, IDbCommand query)
        {
            return RunDynamicSelect(type, database, query) as List<object>;
        }

        public static List<T> RunSelect<T>(IDatabase database, IDbCommand query) where T : new()
        {
            return RunSelect(typeof(T), database, query).Cast<T>().ToList();
        }

        public static List<object> RunSelect(Type type, IDatabase database, string query,
            IEnumerable<IQueryParameter> paramenter)
        {
            return
                database.Run(
                    s =>
                    {
                        IDbCommand command = CreateCommand(s, query);

                        foreach (IQueryParameter item in paramenter)
                            command.Parameters.AddWithValue(item.Name, item.Value, s);
                        return RunSelect(type, database, command);
                    }
                    );
        }

        public static List<T> RunSelect<T>(IDatabase database, string query, IEnumerable<IQueryParameter> paramenter)
            where T : new()
        {
            return RunSelect(typeof(T), database, query, paramenter).Cast<T>().ToList();
        }

        private List<object> RunSelect(Type type, IDbCommand command)
        {
            return RunSelect(type, Database, command);
        }

        private List<T> RunSelect<T>(IDbCommand command) where T : new()
        {
            return RunSelect(typeof(T), Database, command).Cast<T>().ToList();
        }

        #endregion

        #region SelectWhereCommands

        public List<object> SelectWhere(Type type, String @where)
        {
            IDbCommand query = CreateSelect(type, Database, @where);
            return RunSelect(type, query);
        }

        public List<T> SelectWhere<T>(String @where) where T : new()
        {
            return SelectWhere(typeof(T), @where).Cast<T>().ToList();
        }

        public List<object> SelectWhere(Type type, String @where, IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand query = CreateSelect(type, Database, @where, paramenter);
            return RunSelect(type, query);
        }

        public List<T> SelectWhere<T>(String @where, IEnumerable<IQueryParameter> paramenter) where T : new()
        {
            return SelectWhere(typeof(T), where, paramenter).Cast<T>().ToList();
        }

        public List<object> SelectWhere(Type type, String @where, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = EnumarateFromDynamics(paramenter);
            return SelectWhere(type, where, enumarateFromDynamics);
        }

        public List<T> SelectWhere<T>(String @where, dynamic paramenter) where T : new()
        {
            List<object> selectWhere = SelectWhere(typeof(T), @where, paramenter);
            return selectWhere.Cast<T>().ToList();
        }

        #endregion

        #region PrimetivSelects

        private IEnumerable<object> RunPrimetivSelect(Type type, string query)
        {
            return
                Database.Run(
                    s =>
                        s.GetEntitiesList(CreateCommand(s, query), e => e[0])
                    ).ToList();
        }

        private List<T> RunPrimetivSelect<T>(string query) where T : class
        {
            return RunPrimetivSelect(typeof(T), query).Cast<T>().ToList();
        }

        public List<object> SelectNative(Type type, string query)
        {
            return Select(type, Database, CreateCommand(Database, query));
        }

        public List<T> SelectNative<T>(string query) where T : class
        {
            return SelectNative(typeof(T), query).Cast<T>().ToList();
        }

        public static List<object> SelectNative(Type type, IDatabase database, IDbCommand command)
        {
            List<object> objects = RunSelect(type, database, command);

            foreach (object model in objects)
                model.LoadNavigationProps(database);

            return objects;
        }

        public List<object> SelectNative(Type type, IDbCommand command)
        {
            return SelectNative(type, Database, command);
        }

        public List<object> SelectNative(Type type, string query, IEnumerable<IQueryParameter> paramenter)
        {
            IDbCommand dbCommand = CreateCommandWithParameterValues(query, Database, paramenter);
            return SelectNative(type, dbCommand);
        }

        public List<T> SelectNative<T>(string query, IEnumerable<IQueryParameter> paramenter) where T : new()
        {
            return RunSelect<T>(Database, query, paramenter);
        }

        public List<object> SelectNative(Type type, string query, dynamic paramenter)
        {
            IEnumerable<IQueryParameter> enumarateFromDynamics = EnumarateFromDynamics(paramenter);
            return SelectNative(type, query, enumarateFromDynamics);
        }

        public List<T> SelectNative<T>(string query, dynamic paramenter) where T : new()
        {
            var objects = ((List<object>)SelectNative(typeof(T), query, paramenter));
            return objects.Cast<T>().ToList();
        }

        #endregion


        #region experimental

        private TestQueryProvider _testQueryProvider;

        public DbAccessLayer()
        {
            _testQueryProvider = new TestQueryProvider(this);
        }

        public IQueryable<T> SelectQuery<T>()
        {
            var makeGenericMethod = ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (T));
            var methodCallExpression = Expression.Call(Expression.Constant(this), makeGenericMethod);
            return _testQueryProvider.CreateQuery<T>(methodCallExpression);
        }

        #endregion
    }
}