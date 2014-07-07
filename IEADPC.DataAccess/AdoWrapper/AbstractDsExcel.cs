using System;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text.RegularExpressions;

//using JRO;

//IMEX=1: tells the driver that intermixed data should be treated as text.
//http://support.microsoft.com/kb/257819
//***** IMEX should only used for import!!! *****

//also helpfull:    http://www.codeproject.com/KB/office/excel_using_oledb.aspx
//and ...:          http://www.brianpeek.com/blog/archive/2006/04/18/415.aspx
//and ...:          http://www.codeproject.com/KB/miscctrl/Excel_data_access.aspx

namespace DataAccess.AdoWrapper
{
    internal abstract class AbstractDsExcel : IDatabaseStrategy
    {
        protected string _conn_str = string.Empty;
        protected string _dbfile = string.Empty;

        protected AbstractDsExcel(string strAccessFilename, bool bAssumeHeader, bool bUseIMEX)
        {
            _dbfile = strAccessFilename;

            _conn_str = GetConnectionString(strAccessFilename, bAssumeHeader, bUseIMEX);
        }

        protected AbstractDsExcel(string strAccessFilename, string conn_str)
        {
            _dbfile = strAccessFilename;

            _conn_str = conn_str;
        }

        #region IDatabaseStrategy Members

        public string ConnectionString
        {
            get { return _conn_str; }
        }

        public string DatabaseFile
        {
            get { return _dbfile; }
        }

        public string ServerName
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        /// <summary>
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strDest"></param>
        public void CompactDatabase(string strSource, string strDest)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public void ShrinkDatabase(string strConnectionString)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public void PrepareQuery(IDbConnection conn, string strSql)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public IDbConnection CreateConnection()
        {
            return new OleDbConnection(_conn_str);
        }

        public IDbCommand CreateCommand(string strSql, IDbConnection conn)
        {
            var cmd = new OleDbCommand(strSql);
            cmd.Connection = (OleDbConnection) conn;
            return cmd;
        }

        public IDbCommand CreateCommand(IDbConnection conn, string strSql, params IDbDataParameter[] fields)
        {
            var cmd = (OleDbCommand) CreateCommand(strSql, conn);
            for (int i = 0; i < fields.Length; i++)
            {
                cmd.Parameters.Add(new OleDbParameter(fields[i].ParameterName, (OleDbType) fields[i].DbType,
                                                      fields[i].Size));
            }
            return cmd;
        }

        public IDbDataParameter CreateParameter_VarChar(string strName, int iSize, bool nullable = false)
        {
            return new OleDbParameter(strName, OleDbType.VarChar, iSize) {IsNullable = nullable};
        }

        public IDbDataParameter CreateParameter_NVarChar(string strName, int iSize, bool nullable = false)
        {
            throw new NotSupportedException("CreateParameter_NVarChar is not supported");
        }

        public IDbDataParameter CreateParameter_NVarChar_MAX(string strName)
        {
            throw new NotSupportedException("CreateParameter_NVarChar is not supported");
        }

        public IDbDataParameter CreateParameter_Bit(string strName, bool nullable = false)
        {
            return new OleDbParameter(strName, OleDbType.Boolean) {IsNullable = nullable};
        }

        public IDbDataParameter CreateParameter_Int(string strName, bool nullable = false)
        {
            return new OleDbParameter(strName, OleDbType.Integer) {IsNullable = nullable};
        }

        public IDbDataParameter CreateParameter_SmallInt(string strName)
        {
            return new OleDbParameter(strName, OleDbType.SmallInt);
        }

        public IDbDataParameter CreateParameter_BigInt(string strName)
        {
            return new OleDbParameter(strName, OleDbType.BigInt);
        }

        public IDbDataParameter CreateParameter_DateTime(string strName, bool nullable = false)
        {
            return new OleDbParameter(strName, OleDbType.Date) {IsNullable = nullable};
        }

        public IDbDataParameter CreateParameter_Time(string strName, bool nullable = false)
        {
            return new OleDbParameter(strName, OleDbType.DBTime) {IsNullable = nullable};
        }

        public IDbDataParameter CreateParameter_SmallDateTime(string strName)
        {
            return new OleDbParameter(strName, OleDbType.Date);
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            var adapter = new OleDbDataAdapter();
            adapter.SelectCommand = (OleDbCommand) cmd;
            return adapter;
        }

        public DataTable CreateDataTable(string name, IDbCommand cmd)
        {
            using (var adapter = new OleDbDataAdapter())
            {
                adapter.SelectCommand = (OleDbCommand) cmd;

                var table = new DataTable(name);
                adapter.Fill(table);

                cmd.Connection.Close();
                cmd.Connection.Dispose();

                cmd.Dispose();

                return table;
            }
        }

        public void Import(DataTable dt, IDbCommand cmd)
        {
            using (var adapter = new OleDbDataAdapter())
            {
                adapter.SelectCommand = (OleDbCommand) cmd;

                var combuild = new OleDbCommandBuilder(adapter);
                //Debug.Writeline(combuild.GetInsertCommand.CommandText)
                //MsgBox.Show(combuild.GetInsertCommand().CommandText);

                foreach (DataRow row in dt.Rows)
                    row.SetAdded();

                adapter.Update(dt);
            }
        }

        public string GetTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
        }

        public string GetTimeStamp(DateTime dt)
        {
            return string.Format("{0:d2}.{1:d2}.{2:d4} {3:d2}:{4:d2}:{5:d2}",
                                 dt.Day, dt.Month, dt.Year,
                                 dt.Hour, dt.Minute, dt.Second);
        }

        public string[] GetTables(IDbConnection conn, String strFilter)
        {
            if (strFilter != "%") throw new NotSupportedException("filter arent't supported for excel");

            if (conn.State != ConnectionState.Open)
                conn.Open();
            var cn = (OleDbConnection) conn;

            DataTable dtXlsSchema = cn.GetOleDbSchemaTable(
                OleDbSchemaGuid.Tables,
                new Object[] {null, null, null, "TABLE"});

            return dtXlsSchema.Rows
                              .Cast<DataRow>()
                              .Select(row => row["Table_Name"].ToString())
                              .ToArray();
        }

        /// <summary>
        ///     HHH
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="strTableName"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public string[] GetTableColumns(IDbConnection conn, string strTableName, params object[] exclude)
        {
            //http://www.velocityreviews.com/forums/t648333-oledbdataadapter-issue.html

            if (strTableName[strTableName.Length - 1] != '$')
                strTableName += "$";
            if (Regex.IsMatch(strTableName, @"\s"))
                strTableName = string.Format("'{0}'", strTableName);

            var cn = (OleDbConnection) conn;

            DataTable dtXlsSchema = cn.GetOleDbSchemaTable(
                OleDbSchemaGuid.Columns,
                new Object[] {null, null, strTableName, null});

            string[] res = dtXlsSchema.Rows
                                      .Cast<DataRow>()
                                      .Where(row => row != null)
                                      .Select(row => row["Column_Name"].ToString())
                                      .ToArray();
            if (res.Count() < 255) return res;

            //exctended approach

            return res;
        }

        public int DropTable(IDbConnection conn, string strTableName)
        {
            throw new NotImplementedException(
                "Jet OLEDB provider has no full support for the DROP statement when it comes to Excel files.");
        }

        public bool SupportsView(IDbConnection conn, string strName)
        {
            throw new NotSupportedException();
        }

        public bool SupportsStoredProcedure(IDbConnection conn, string strName)
        {
            throw new NotSupportedException();
        }

        public IDbCommand GetlastInsertedID_Cmd(IDbConnection conn)
        {
            throw new NotSupportedException("not supported for excel");
        }

        public string GetViewsSql(String strName)
        {
            throw new NotSupportedException("not supported for excel");
        }

        public string GetStoredProcedureSql(String strName)
        {
            throw new NotSupportedException("not supported for excel");
        }

        public abstract object Clone();

        #endregion

        protected abstract string GetConnectionString(string strAccessFilename, bool bAssumeHeader, bool bUseIMEX);
    }

    ///
}

///