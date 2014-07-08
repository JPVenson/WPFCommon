using System;
using System.Data;
using System.Linq;
using DataAccess.AdoWrapper;
using DataAccess.ModelsAnotations;

namespace DataAccess.Manager
{
    public partial class DbAccessLayer
    {
        public static void Update<T>(T entry, IDatabase db) where T : new()
        {
            db.Run(s => { s.ExecuteNonQuery(CreateUpdate(entry, s)); });
        }

        public void Update<T>(T entry) where T : new()
        {
            Update(entry, Database);
        }

        public T Refresh<T>(T entry) where T : new()
        {
            return Select<T>(entry.GetPK<T, long>(), Database);
        }

        public static IDbCommand CreateUpdate<T>(T entry, IDatabase batchRemotingDb) where T : new()
        {
            Type type = typeof(T);
            string pk = type.GetPK();

            string[] ignore =
                type.GetProperties()
                    .Where(s => s.CheckForPK() || s.GetCustomAttributes(false).Any(e => e is InsertIgnore))
                    .Select(s => s.Name)
                    .Concat(CreateIgnoreList(type))
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
    }
}
