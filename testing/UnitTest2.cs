using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace testing
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            var manager = new DbAccessLayer();
            manager.Database =
                Database.CreateMSSQL(
                    "Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False");

        }


     
    }
}
