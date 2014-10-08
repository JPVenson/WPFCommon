using System;
using System.Runtime.Serialization;

namespace JPB.Communication.ComBase.Messages
{
    [Serializable]
    public class RequstMessage : MessageBase
    {
        public RequstMessage()
        {
            ResponseFor = Guid.NewGuid();
        }

        public RequstMessage(SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            ResponseFor = (Guid)info.GetValue("ResponseFor", typeof(Guid));
        }
        
        public Guid ResponseFor { get; set; }
        
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ResponseFor", ResponseFor, ResponseFor.GetType());
            base.GetObjectData(info, context);
        }
    }
}