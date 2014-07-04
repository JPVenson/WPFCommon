using System;

namespace IEADPC.DataAccess.AdoWrapper
{
    internal class DsExcel_XLSX : AbstractDsExcel
    {
        public DsExcel_XLSX(string strFilename, bool bAssumeHeader, bool bUseIMEX)
            : base(strFilename, bAssumeHeader, bUseIMEX)
        {
        }

        private DsExcel_XLSX(string strFilename, string conn_str)
            : base(strFilename, conn_str)
        {
        }

        protected override string GetConnectionString(string strAccessFilename, bool bAssumeHeader, bool bUseIMEX)
        {
            return string.Format(
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0 Xml;{1}HDR={2}"";",
                strAccessFilename,
                (bUseIMEX) ? "IMEX=1;" : String.Empty,
                (bAssumeHeader) ? "Yes" : "No");
        }

        public override object Clone()
        {
            return new DsExcel_XLSX(_dbfile, _conn_str);
        }
    }
}