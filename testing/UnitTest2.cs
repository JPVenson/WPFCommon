using System;
using JPB.DataAccess.AdoWrapper;
using JPB.DataAccess.Manager;
using JPB.DataAccess.ModelsAnotations;
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

    [Serializable]
    [ForModel("Users")]
    public class User
    {
        [PrimaryKey]
        [ForModel("User_ID")]
        public long UserId { get; set; }

        [ForModel("UserName")]
        public string Name { get; set; }

        public long? ID_Image { get; set; }

        [ForeignKey("ID_Image")]
        public virtual Image Img { get; set; }
    }

    [Serializable]
    [ForModel("Images")]
    public class Image
    {
        [PrimaryKey]
        [ForModel("Image_ID")]
        public long Id { get; set; }

        [ForModel("Content")]
        public string Text { get; set; }
    }
}
