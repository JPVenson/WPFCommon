using System;
using System.Collections.Generic;

namespace IEADPC.BatchRemoting.DataAccess.Models
{
    public partial class MAP_BatchServer_BatchType
    {
        public int MAP_BatchServer_BatchType_ID { get; set; }
        public int ID_BatchType { get; set; }
        public int ID_BatchServer { get; set; }
        public byte[] RowState { get; set; }
    }
}
