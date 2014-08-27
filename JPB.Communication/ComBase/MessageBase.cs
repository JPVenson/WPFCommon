#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:24

#endregion

using System;
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

        public object Message { get; set; }
        public object InfoState { get; set; }

        public string Sender { get; internal set; }
        public string Reciver { get; internal set; }

        public Guid Id { get; internal set; }
        public DateTime RecievedAt { get; set; }
        public DateTime SendAt { get; internal set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Message", Message, Message.GetType());
            info.AddValue("InfoState", InfoState, InfoState.GetType());
            info.AddValue("ID", Id, Id.GetType());
            info.AddValue("RecievedAt", RecievedAt, RecievedAt.GetType());
        }
    }
}