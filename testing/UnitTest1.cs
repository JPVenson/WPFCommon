//using System.Collections.Generic;
//using System.Linq;
//using DataAccess.AdoWrapper;
//using DataAccess.Manager;
//using DataAccess.ModelsAnotations;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace testing
//{
//    [TestClass]
//    public class UnitTest1
//    {
//        public string testName = "TestDD";

//        [TestMethod]
//        public void CheckDbAccess()
//        {
//            var dbaccess = new DbAccessLayer();
//            dbaccess.Database = Database.Create(new Mysql("192.168.1.4", "test", "Any", ""));

//            dbaccess.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users"));

//            CheckInserts(dbaccess);
//            CheckSelects(dbaccess);

//            //var users = dbaccess.Select<User>();
//            //Assert.IsTrue(users.Any());

//            //var first = users.First();
//            //Assert.AreEqual(first.Name, user.Name);

//            dbaccess.Database.Run(s => s.ExecuteNonQuery("DELETE FROM users"));
//        }

//        public void CheckInserts(DbAccessLayer access)
//        {
//            var user = new User();
//            user.Name = testName;
//            access.Insert(user);
//            user.Name += "_1";
//            access.InsertRange(new List<User> { user });
//            user.Name += "_2";

//            var img = new Image();
//            img.Text = "BLA";
//            img = access.InsertWithSelect(img);

//            user.ID_Image = img.Id;

//            var updatedUser = access.InsertWithSelect(user);

//            updatedUser.ID_Image = img.Id;
//            access.Update(updatedUser);
//        }

//        public void CheckSelects(DbAccessLayer access)
//        {
//            var @select = access.Select<User>();
//            Assert.AreEqual(@select.Count, 3);

//            long lastID = (long)access.Database.Run(s => s.GetSkalar("SELECT User_ID FROM users limit 1"));
//            long count = (long)access.Database.Run(s => s.GetSkalar("SELECT COUNT(*) FROM users"));

//            var user = access.Select<User>(lastID);
//            Assert.AreEqual(user.Name, testName);

//            var users = access.SelectNative<User>("SELECT * FROM users");
//            Assert.AreEqual(users.Count, 3);

//            var list = access.SelectNative<User>("SELECT * FROM users us WHERE us.User_ID = @test", new { test = lastID }).FirstOrDefault();
//            Assert.AreEqual(list.UserId, lastID);

//            var selectWhere = access.SelectWhere<User>("AS s WHERE s.User_ID != 0");
//            Assert.AreEqual(count, selectWhere.Count);

//            var @where = access.SelectWhere<User>("AS s WHERE s.User_ID = @testVar", new { testVar = lastID }).FirstOrDefault();
//            Assert.AreEqual(@where.UserId, lastID);




//        }
//    }

//    [ForModel("Users")]
//    public class User
//    {
//        [PrimaryKey]
//        [ForModel("User_ID")]
//        public long UserId { get; set; }

//        [ForModel("UserName")]
//        public string Name { get; set; }

//        public long? ID_Image { get; set; }

//        [ForeignKey("ID_Image")]
//        public virtual Image Img { get; set; }
//    }

//    [ForModel("Images")]
//    public class Image
//    {
//        [PrimaryKey]
//        [ForModel("Image_ID")]
//        public long Id { get; set; }

//        [ForModel("Content")]
//        public string Text { get; set; }
//    }
//}
