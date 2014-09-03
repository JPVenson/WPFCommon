using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Interface;
using JPB.Communication.Security;

namespace JPB.Communication.ComBase
{
    public class Networkbase
    {
        public static TcpMessage DeSerialize(string source)
        {
            try
            {
                using (var textReader = new StringReader(source))
                {
                    var deserializer = new XmlSerializer(typeof(TcpMessage));
                    var tcpMessage = (TcpMessage)deserializer.Deserialize(textReader);

                    if (string.IsNullOrEmpty(tcpMessage.MessageSecType))
                    {
                        tcpMessage.MessageBase = SecurityManager.DecryptMessage(tcpMessage.MessageBase, tcpMessage.MessageSecType);
                    }
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

        public byte[] SaveMessageBaseAsBinary(MessageBase A, ISecureMessage provider)
        {
            using (var fs = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, A);
                var array = fs.ToArray();
                if (provider != null)
                {
                    return provider.EncryptMessage(array);
                }
                return array;
            }
        }

        public byte[] SaveMessageBaseAsBinary(MessageBase A)
        {
            return SaveMessageBaseAsBinary(A, null);
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

        protected TcpMessage Wrap(MessageBase message, ISecureMessage provider)
        {
            var mess = new TcpMessage();
            if (provider != null)
                mess.MessageSecType = provider.GeneratePublicId();

            mess.MessageBase = SaveMessageBaseAsBinary(message);
            mess.Reciver = message.Reciver;
            mess.Sender = message.Sender;
            return mess;
        }

        protected TcpMessage Wrap(MessageBase message)
        {
            return Wrap(message, null);
        }
    }
}