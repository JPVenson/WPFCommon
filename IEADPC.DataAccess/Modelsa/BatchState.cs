using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class BatchState
    {
        public int BatchState_ID { get; set; }
        public string Value { get; set; }
        public byte[] RowState { get; set; }
    }
}
