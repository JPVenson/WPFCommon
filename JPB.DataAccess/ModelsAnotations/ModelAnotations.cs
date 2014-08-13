using System;

namespace JPB.DataAccess.ModelsAnotations
{
    public class DataAccessAttribute : Attribute
    {
    }

    public class InsertIgnore : DataAccessAttribute
    {
    }

    public class ForeignKeyAttribute : InsertIgnore
    {
        public ForeignKeyAttribute(string keyname)
        {
            KeyName = keyname;
        }

        public string KeyName { get; set; }
    }

    public class PrimaryKeyAttribute : DataAccessAttribute
    {
    }

    public class ForModel : DataAccessAttribute
    {
        public ForModel(string alternatingName)
        {
            AlternatingName = alternatingName;
        }

        public string AlternatingName { get; set; }
    }
}