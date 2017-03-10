using System;
using System.Data;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace testing
{
    [ForModel("Users")]
    public class User
    {
        [ObjectFactoryMethod]
        public User(IDataRecord record)
        {
            UserId = (long)record["User_ID"];
            Name = (string)record["UserName"];
            var o = record["ID_Image"];
            ID_Image = (long?)o;
            RowBla = (byte[])record["RowState"];
        }

        public User()
        {
            
        }


        [ForModel("UserName")]
        public string Name { get; set; }

        public long? ID_Image { get; set; }

        [PrimaryKey]
        [ForModel("User_ID")]
        public long UserId { get; set; }

        [RowVersion]
        [ForModel("RowState")]
        public byte[] RowBla { get; set; }
        
        [SelectFactoryMethod()]
        public static IQueryFactoryResult CreateQuery()
        {
            return new QueryFactoryResult("SELECT * FROM Users");
        }

        [UpdateFactoryMethod()]
        public string UpdateQuery()
        {
            return string.Empty;
        }
    }
}