﻿using System;

namespace DataAccess.ModelsAnotations
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
        public PrimaryKeyAttribute()
        {

        }
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