using System.Collections.Generic;
using System.Linq;
using DataAccess.AdoWrapper;
using DataAccess.Manager;
using DataAccess.ModelsAnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace testing
{
    [TestClass]
    public class UnitTest1
    {
        public string testName = "TestDD";
        private static DbAccessLayer dbaccess;

        [ClassCleanup]
        public static void CleanUp()
        {
            dbaccess.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users"));
        }

        [TestInitialize()]
        public void CheckDbAccess()
        {
            dbaccess = new DbAccessLayer();
            dbaccess.Database =
                Database.CreateMSSQL(
                    "Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False");

        }

        [TestMethod]
        public void CheckLinq()
        {
            IQueryable<User> selectQuery = dbaccess.SelectQuery<User>();
            User[] array = selectQuery.WhereSql(s => s.Name == "Test").OrSql(s => s.UserId == 0).AndSql(s => s.Name == "!").ToArray();

            var res = dbaccess.SelectQuery<User>()
                .WhereSql(s => s.Name == testName).ToArray();
            Assert.AreEqual(res.Length, 1);


            var firstOrDefault = res.FirstOrDefault();
            Assert.IsNotNull(firstOrDefault);
            Assert.AreEqual(firstOrDefault.Name, testName);
        }

        [TestMethod]
        public void CheckInserts()
        {
            DbAccessLayer access = dbaccess;

            var user = new User();
            user.Name = testName;
            access.Insert(user);
            user.Name += "_1";
            access.InsertRange(new List<User> { user });
            user.Name += "_2";

            var img = new Image();
            img.Text = "BLA";
            img = access.InsertWithSelect(img);

            user.ID_Image = img.Id;

            var updatedUser = access.InsertWithSelect(user);

            updatedUser.ID_Image = img.Id;
            access.Update(updatedUser);
        }

        [TestMethod]
        public void CheckSelects()
        {
            DbAccessLayer access = dbaccess;

            var @select = access.Select<User>();
            Assert.AreEqual(@select.Count, 3);

            long lastID = (long)access.Database.Run(s => s.GetSkalar("SELECT TOP 1 User_ID FROM Users"));
            long count = (int)access.Database.Run(s => s.GetSkalar("SELECT COUNT(*) FROM Users"));

            var user = access.Select<User>(lastID);
            Assert.AreEqual(user.Name, testName);

            var users = access.SelectNative<User>("SELECT * FROM Users");
            Assert.AreEqual(users.Count, 3);

            var list = access.SelectNative<User>("SELECT * FROM Users us WHERE us.User_ID = @test", new { test = lastID }).FirstOrDefault();
            Assert.AreEqual(list.UserId, lastID);

            var selectWhere = access.SelectWhere<User>("AS s WHERE s.User_ID != 0");
            Assert.AreEqual(count, selectWhere.Count);

            var @where = access.SelectWhere<User>("AS s WHERE s.User_ID = @testVar", new { testVar = lastID }).FirstOrDefault();
            Assert.AreEqual(@where.UserId, lastID);
        }
    }
}
