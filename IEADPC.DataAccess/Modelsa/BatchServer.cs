using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class BatchServer
    {
        public int BatchServer_ID { get; set; }
        public string ServerName { get; set; }
        public string ServerIP { get; set; }
        public bool Working { get; set; }
        public bool IsOnline { get; set; }
        public byte[] RowState { get; set; }
    }
}
