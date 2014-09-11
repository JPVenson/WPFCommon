using System;
using System.Data;
using JPB.DataAccess.ModelsAnotations;
using JPB.DataAccess.QueryFactory;

namespace testing
{
    [Serializable]
    [ForModel("Users")]
    public class User
    {
        [ObjectFactoryMehtod]
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

        [PrimaryKey]
        [ForModel("User_ID")]
        public long UserId { get; set; }

        [ForModel("UserName")]
        public string Name { get; set; }

        public long? ID_Image { get; set; }

        [RowVersion]
        [ForModel("RowState")]
        public byte[] RowBla { get; set; }
        
        [SelectFactoryMehtod()]
        public static QueryFactoryResult CreateQuery()
        {
            return new QueryFactoryResult("SELECT * FROM Users");
        }

        [UpdateFactoryMehtod()]
        public string UpdateQuery()
        {
            return string.Empty;
        }
    }
}