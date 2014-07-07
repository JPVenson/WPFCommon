using System;
using DataAccess.AdoWrapper;
using DataAccess.Manager;
using JPB.DataAccess.MySQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace testing
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dbaccess = new DbAccessLayer();
            dbaccess.Database = Database.Create(new Mysql("192.168.1.7", "test", "Any", ""));
            bool checkDatabase = dbaccess.CheckDatabase();

            Assert.AreEqual(checkDatabase, true);
        }
    }
}
