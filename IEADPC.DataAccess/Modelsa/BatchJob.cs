using System;
using IEADPC.BatchRemoting.DataAccess.ModelsAnotations;

namespace IEADPC.BatchRemoting.DataAccess.Modelsa
{
    public partial class BatchJob
    {
        [PrimaryKey]
        public int BatchJob_ID { get; set; }
        public Nullable<System.DateTime> ExecutionStartTime { get; set; }
        public Nullable<System.DateTime> ExecutionEndTime { get; set; }
        public string JobName { get; set; }
        public string TargetFileName { get; set; }
        public Nullable<int> DatabaseID { get; set; }
        public string Paramenter { get; set; }
        public string Error { get; set; }
        public bool IsEnabled { get; set; }
        public int ID_JobType { get; set; }
        public Nullable<int> ID_ServerBatch { get; set; }
        public int ID_BatchState { get; set; }
        public byte[] RowState { get; set; }
    }
}
