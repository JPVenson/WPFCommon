using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class ServerBatch
    {
        public int ServerBatch_ID { get; set; }
        public string ExecuteUser { get; set; }
        public string Descriptor { get; set; }
        public Nullable<System.DateTime> ExecutionStartTime { get; set; }
        public Nullable<System.DateTime> ExecutionEndTime { get; set; }
        public bool IsEnabeld { get; set; }
        public bool IsTemplate { get; set; }
        public Nullable<int> ID_Parent { get; set; }
        public Nullable<int> ID_BatchTemplate { get; set; }
        public int ID_BatchState { get; set; }
        public int ID_BatchType { get; set; }
        public Nullable<int> ID_BatchServer { get; set; }
        public int ID_Study { get; set; }
        public byte[] RowState { get; set; }
    }
}
