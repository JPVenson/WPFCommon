#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:24

#endregion

using System;
using System.Runtime.Serialization;

namespace JPB.Communication.ComBase.Messages
{
    [Serializable]
    public class MessageBase : ISerializable
    {
        private object _message;
        private object _infoState;

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

            Sender = (string)info.GetValue("Sender", typeof(string));
            Reciver = (string)info.GetValue("Reciver", typeof(string));
        }

        public object Message
        {
            get { return _message; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", @"Message can not be null");
                _message = value;
            }
        }

        public object InfoState
        {
            get { return _infoState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", @"InfoState can not be null");
                _infoState = value;
            }
        }

        public string Sender { get; internal set; }
        public string Reciver { get; internal set; }

        public Guid Id { get; internal set; }
        public DateTime RecievedAt { get; internal set; }
        public DateTime SendAt { get; internal set; }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (Message == null)
                Message = new object();
            if (InfoState == null)
                InfoState = new object();

            info.AddValue("Reciver", Reciver, Reciver.GetType());
            info.AddValue("Sender", Sender, Sender.GetType());

            info.AddValue("Message", Message, Message.GetType());
            info.AddValue("InfoState", InfoState, InfoState.GetType());
            info.AddValue("ID", Id, Id.GetType());
            info.AddValue("RecievedAt", RecievedAt, RecievedAt.GetType());
        }
    }

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