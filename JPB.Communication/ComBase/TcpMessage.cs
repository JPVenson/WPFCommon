using System;

namespace JPB.Communication.ComBase
{
    public class TcpMessage
    {
        public TcpMessage()
        {
            GUID = Guid.NewGuid();
        }

        public Guid GUID { get; set; }

        public byte[] MessageBase { get; set; }
        public string Sender { get; set; }
        public string Reciver { get; set; }
    }
}