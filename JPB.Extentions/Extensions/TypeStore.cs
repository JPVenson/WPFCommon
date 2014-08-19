using System;
using System.Collections.Generic;

namespace JPB.Extentions.Extensions
{
    [Serializable]
    public class TypeStore
    {
        private Type[] _typen;

        public Type[] Typen
        {
            get { return _typen; }
            set { _typen = value; }
        }
    }
}