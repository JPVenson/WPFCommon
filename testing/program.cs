using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace testing
{
    class program
    {
        public static int Main()
        {
            var unitTest1 = new UnitTest1();
            unitTest1.CheckDbdbaccess();
            //unitTest1.CheckInserts();
            unitTest1.CheckUpdate();
            return 0;
        }
    }
}
