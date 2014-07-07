using System;

namespace DataAccess.AdoWrapper
{
    internal class DsExcel_XLS : AbstractDsExcel
    {
        public DsExcel_XLS(string strFilename, bool bAssumeHeader, bool bUseIMEX)
            : base(strFilename, bAssumeHeader, bUseIMEX)
        {
        }

        private DsExcel_XLS(string strFilename, string conn_str)
            : base(strFilename, conn_str)
        {
        }

        protected override string GetConnectionString(string strAccessFilename, bool bAssumeHeader, bool bUseIMEX)
        {
            return string.Format(
                @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=""Excel 8.0;{1}HDR={2}"";",
                strAccessFilename,
                (bUseIMEX) ? "IMEX=1;" : String.Empty,
                (bAssumeHeader) ? "Yes" : "No");
        }

        public override object Clone()
        {
            return new DsExcel_XLS(_dbfile, _conn_str);
        }
    }
}