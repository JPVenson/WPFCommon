using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class Study
    {
        public int Study_ID { get; set; }
        public string StudyName { get; set; }
        public byte[] RowState { get; set; }
    }
}
