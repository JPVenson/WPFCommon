using System.Collections.Generic;
using System.Linq;
using JPB.DataAccess;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Manager;
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
        public void CheckDbdbaccess()
        {
            dbaccess = new DbAccessLayer();
            dbaccess.Database = Database.CreateMSSQL("Data Source=(localdb)\\Projects;Initial Catalog=TestDB;Integrated Security=True;");
        }

        [TestMethod]
        public void CheckLinq()
        {
            //IQueryable<User> selectQuery = dbaccess.SelectQuery<User>();
            //User[] array = selectQuery.WhereSql(s => s.Name == "Test").OrSql(s => s.UserId == 0).AndSql(s => s.Name == "!").ToArray();

            //var res = dbaccess.SelectQuery<User>()
            //    .WhereSql(s => s.Name == testName).ToArray();
            //Assert.AreEqual(res.Length, 1);
            //var firstOrDefault = res.FirstOrDefault();
            //Assert.IsNotNull(firstOrDefault);
            //Assert.AreEqual(firstOrDefault.Name, testName);
        }

        [TestMethod]
        public void CheckInserts()
        {
            var user = new User();
            user.Name = testName;
            dbaccess.Insert(user);
            user.Name += "_1";
            dbaccess.InsertRange(new List<User> { user });
            user.Name += "_2";

            var img = new Image();
            img.Text = "BLA";
            img = dbaccess.InsertWithSelect(img);

            user.ID_Image = img.Id;

            var updatedUser = dbaccess.InsertWithSelect(user);

            updatedUser.ID_Image = img.Id;
            dbaccess.Update(updatedUser);
        }

        [TestMethod]
        public void CheckSelects()
        {
            var @select = dbaccess.Select<User>();
            Assert.AreEqual(@select.Count, 3);

            long lastID = (long)dbaccess.Database.Run(s => s.GetSkalar("SELECT TOP 1 User_ID FROM Users"));
            long count = (int)dbaccess.Database.Run(s => s.GetSkalar("SELECT COUNT(*) FROM Users"));

            var user = dbaccess.Select<User>(lastID);
            Assert.AreEqual(user.Name, testName);

            var users = dbaccess.SelectNative<User>("SELECT * FROM Users");
            Assert.AreEqual(users.Count, 3);

            var list = dbaccess.SelectNative<User>("SELECT * FROM Users us WHERE us.User_ID = @test", new { test = lastID }).FirstOrDefault();
            Assert.AreEqual(list.UserId, lastID);

            var selectWhere = dbaccess.SelectWhere<User>("AS s WHERE s.User_ID != 0");
            Assert.AreEqual(count, selectWhere.Count);

            var @where = dbaccess.SelectWhere<User>("AS s WHERE s.User_ID = @testVar", new { testVar = lastID }).FirstOrDefault();
            Assert.AreEqual(@where.UserId, lastID);
        }

        [TestMethod]
        public void CheckUpdate()
        {
            var @select = dbaccess.Select<User>();

            var enumerable = @select.Take(3).ToArray();
            var users = new List<User>();

            foreach (var user in enumerable)
            {
                user.Name = user.Name + "_new";
                users.Add(new User() { UserId = user.UserId, RowBla = user.RowBla });
            }

            foreach (var user in enumerable)
            {
                dbaccess.Update(user);
            }

            foreach (var user in users)
            {
                dbaccess.RefreshKeepObject(user);
            }
        }
    }
}
