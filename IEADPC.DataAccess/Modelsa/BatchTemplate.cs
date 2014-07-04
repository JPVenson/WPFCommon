using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class BatchTemplate
    {
        public int BatchTemplate_ID { get; set; }
        public string ExecuteUser { get; set; }
        public string Descriptor { get; set; }
        public Nullable<bool> IsInUse { get; set; }
    }
}
