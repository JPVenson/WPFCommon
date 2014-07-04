using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class Error
    {
        public int Errors_ID { get; set; }
        public Nullable<int> ProgramRunID { get; set; }
        public string ErrorMessage { get; set; }
    }
}
