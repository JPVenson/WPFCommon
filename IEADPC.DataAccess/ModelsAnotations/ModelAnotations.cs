using System;

namespace IEADPC.DataAccess.ModelsAnotations
{
    public class ForModel : Attribute
    {
        public ForModel(string alternatingName)
        {
            AlternatingName = alternatingName;
        }

        public string AlternatingName { get; set; }
    }
}