using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Xml.Serialization;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    public delegate void MessageDelegate(MessageBase mess, short port);

    public abstract class Networkbase
    {
        public abstract short Port { get; internal set; }

        public static event MessageDelegate OnNewItemLoadedSuccess;
        public static event EventHandler<string> OnNewItemLoadedFail;
        public static event EventHandler<string> OnIncommingMessage;
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

        protected virtual void RaiseIncommingMessage(string strReceived)
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

        internal static Encoding Encoding = Encoding.UTF8;

        public static TcpMessage DeSerialize(string source)
        {
            try
            {
                using (var textReader = new StringReader(source))
                {
                    var deserializer = new XmlSerializer(typeof(TcpMessage));
                    var tcpMessage = (TcpMessage)deserializer.Deserialize(textReader);
                    return tcpMessage;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static byte[] Serialize(TcpMessage a)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new XmlSerializer(a.GetType());
                serializer.Serialize(stream, a);
                return stream.ToArray();
            }
        }

        public byte[] SaveMessageBaseAsBinary(MessageBase A)
        {
            using (var fs = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, A);
                var array = fs.ToArray();
                return array;
            }
        }

        public MessageBase LoadMessageBaseFromBinary(Byte[] source)
        {
            using (var memst = new MemoryStream(source))
            {
                var formatter = new BinaryFormatter();
                var deserialize = (MessageBase)formatter.Deserialize(memst);
                return deserialize;
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