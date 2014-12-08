using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Xml.Serialization;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Properties;

namespace JPB.Communication.ComBase
{
    public delegate void MessageDelegate(MessageBase mess, short port);

    public abstract class Networkbase
    {
        protected Networkbase()
        {
            Serlilizer = DefaultMessageSerializer;
        }

        public abstract short Port { get; internal set; }

        public IMessageSerializer Serlilizer { get; set; }

        public static readonly IMessageSerializer DefaultMessageSerializer = new DefaultMessageSerlilizer();
        public static readonly IMessageSerializer CompressedDefaultMessageSerializer = new BinaryCompressedMessageSerilalizer();
        public static readonly IMessageSerializer JsonMessageSerializer = new MessageJsonSerlalizer();

        public static event MessageDelegate OnNewItemLoadedSuccess;
        public static event EventHandler<string> OnNewItemLoadedFail;
        public static event EventHandler<TcpMessage> OnIncommingMessage;
        public static event MessageDelegate OnMessageSend;

        protected virtual void RaiseMessageSended(MessageBase message)
        {
            try
            {
                var handler = OnMessageSend;
                if (handler != null)
                    handler(message, Port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected virtual void RaiseIncommingMessage(TcpMessage strReceived)
        {
            try
            {
                var handler = OnIncommingMessage;
                if (handler != null)
                    handler(this, strReceived);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected virtual void RaiseNewItemLoadedFail(string strReceived)
        {
            try
            {
                var handler = OnNewItemLoadedFail;
                if (handler != null)
                    handler(this, strReceived);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected virtual void RaiseNewItemLoadedSuccess(MessageBase loadMessageBaseFromBinary)
        {
            try
            {
                var handler = OnNewItemLoadedSuccess;
                if (handler != null)
                    handler(loadMessageBaseFromBinary, Port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public TcpMessage DeSerialize(byte[] source)
        {
            try
            {
                return this.Serlilizer.DeSerializeMessage(source);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public byte[] Serialize(TcpMessage a)
        {
            try
            {
                return this.Serlilizer.SerializeMessage(a);
            }
            catch (Exception e)
            {
                return new byte[0];
            }
        }

        public byte[] SaveMessageBaseAsBinary(MessageBase A)
        {
            try
            {
                return this.Serlilizer.SerializeMessageContent(A);
            }
            catch (Exception e)
            {
                return new byte[0];
            }
        }

        public MessageBase LoadMessageBaseFromBinary(byte[] source)
        {
            try
            {
                return this.Serlilizer.DeSerializeMessageContent(source);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected TcpMessage Wrap(MessageBase message)
        {
            var mess = new TcpMessage();
            mess.MessageBase = SaveMessageBaseAsBinary(message);
            mess.Reciver = message.Reciver;
            mess.Sender = message.Sender;
            return mess;
        }
    }
}