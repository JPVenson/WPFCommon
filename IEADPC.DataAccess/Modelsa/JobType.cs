using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class JobType
    {
        public int JobType_ID { get; set; }
        public string JobTypeKey { get; set; }
        public string DisplayName { get; set; }
        public byte[] RowState { get; set; }
    }
}
