using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class BatchType
    {
        public int BatchType_ID { get; set; }
        public string BatchTypeKey { get; set; }
        public string BatchTypeProgram { get; set; }
        public string BatchTypeCommand { get; set; }
        public string DisplayName { get; set; }
        public byte[] RowState { get; set; }
    }
}
