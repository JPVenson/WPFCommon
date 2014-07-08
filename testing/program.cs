using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing
{
    class program
    {
        public static int Main()
        {
            var test = new UnitTest1();
            test.CheckDbAccess();
            return 0;
        }
    }
}
