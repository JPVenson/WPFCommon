using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.AdoWrapper;
using DataAccess.Manager;
using DataAccess.ModelsAnotations;
using JPB.DataAccess.MySQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace testing
{
    [TestClass]
    public class UnitTest1
    {
        public string testName = "TestDD";

        [TestMethod]
        public void CheckDbAccess()
        {
            var dbaccess = new DbAccessLayer();
            dbaccess.Database = Database.Create(new Mysql("192.168.1.7", "test", "Any", ""));
            bool checkDatabase = dbaccess.CheckDatabase();
            Assert.AreEqual(checkDatabase, true);

            dbaccess.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users"));

            CheckInserts(dbaccess);
            CheckSelects(dbaccess);

            //var users = dbaccess.Select<User>();
            //Assert.IsTrue(users.Any());

            //var first = users.First();
            //Assert.AreEqual(first.Name, user.Name);

            dbaccess.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users"));
        }

        public void CheckInserts(DbAccessLayer access)
        {
            var user = new User();
            user.Name = testName;
            access.Insert(user);
            user.Name += "_1";
            access.InsertRange(new List<User> { user });
            user.Name += "_2";
            var insertWithSelect = access.InsertWithSelect(user);
            Assert.AreEqual(insertWithSelect.Name, user.Name);
        }

        public void CheckSelects(DbAccessLayer access)
        {
            var @select = access.Select<User>();
            Assert.AreEqual(@select.Count, 3);

            int lastID = (int)access.Database.Run(s => s.GetSkalar("SELECT User_ID FROM users limit 1"));
            long count = (long)access.Database.Run(s => s.GetSkalar("SELECT COUNT(*) FROM users"));

            var user = access.Select<User>(lastID);
            Assert.AreEqual(user.Name, testName);

            var users = access.SelectNative<User>("SELECT * FROM users");
            Assert.AreEqual(users.Count, 3);

            var list = access.SelectNative<User>("SELECT * FROM users us WHERE us.User_ID = @test", new { test = lastID }).FirstOrDefault();
            Assert.AreEqual(list.UserId, lastID);

            var selectWhere = access.SelectWhere<User>("AS s WHERE s.User_ID != 0");
            Assert.AreEqual(count, selectWhere.Count);

            var @where = access.SelectWhere<User>("AS s WHERE s.User_ID = @testVar", new { testVar = lastID }).FirstOrDefault();
            Assert.AreEqual(@where.UserId, lastID);
        }
    }

    [ForModel("Users")]
    public class User
    {
        [PrimaryKey]
        [ForModel("User_ID")]
        public long UserId { get; set; }

        [ForModel("UserName")]
        public string Name { get; set; }

        [ForeignKey("Image_ID")]
        public virtual Image Image { get; set; }
    }

    [ForModel("Images")]
    public class Image
    {
        [PrimaryKey]
        [ForModel("Image_ID")]
        public long Id { get; set; }

        [ForModel("ID_User")]
        public long UserID { get; set; }
    }
}
