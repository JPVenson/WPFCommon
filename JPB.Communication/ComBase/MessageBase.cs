#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:24

#endregion

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace JPB.Communication.ComBase
{
    [Serializable]
    public class MessageBase : ISerializable
    {
        public MessageBase()
        {
            Message = new object();
            Id = Guid.NewGuid();
        }

        public MessageBase(object mess)
            : this(Guid.NewGuid())
        {
            Message = mess ?? new object();
        }

        public MessageBase(Guid guid)
        {
            Id = guid;
        }

        internal MessageBase(SerializationInfo info,
            StreamingContext context)
        {
            Message = info.GetValue("Message", typeof(object));
            InfoState = info.GetValue("InfoState", typeof(object));
            Id = (Guid)info.GetValue("ID", typeof(Guid));
            RecievedAt = (DateTime)info.GetValue("RecievedAt", typeof(DateTime));
        }

        public object Message { get; set; }
        public object InfoState { get; set; }

        public string Sender { get; internal set; }
        public string Reciver { get; internal set; }

        public Guid Id { get; internal set; }
        public DateTime RecievedAt { get; internal set; }
        public DateTime SendAt { get; internal set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (Message == null)
                Message = new object();
            if (InfoState == null)
                InfoState = new object();

            info.AddValue("Message", Message, Message.GetType());
            info.AddValue("InfoState", InfoState, InfoState.GetType());
            info.AddValue("ID", Id, Id.GetType());
            info.AddValue("RecievedAt", RecievedAt, RecievedAt.GetType());
        }
    }
}