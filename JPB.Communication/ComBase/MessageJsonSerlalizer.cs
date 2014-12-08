using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    public class MessageJsonSerlalizer : IMessageSerializer
    {
        public byte[] SerializeMessage(TcpMessage a)
        {
            using (var memstream = new MemoryStream())
            {
                var json = new DataContractJsonSerializer(typeof(TcpMessage));
                json.WriteObject(memstream, a);
                return memstream.ToArray();
            }
        }

        public byte[] SerializeMessageContent(MessageBase A)
        {
            using (var memstream = new MemoryStream())
            {
                var json = new DataContractJsonSerializer(typeof(MessageBase));
                json.WriteObject(memstream, A);
                return memstream.ToArray();
            }
        }

        public TcpMessage DeSerializeMessage(byte[] source)
        {
            using (var memstream = new MemoryStream(source))
            {
                var json = new DataContractJsonSerializer(typeof(TcpMessage));
                return (TcpMessage)json.ReadObject(memstream);
            }
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            using (var memstream = new MemoryStream(source))
            {
                var json = new DataContractJsonSerializer(typeof(MessageBase));
                return (MessageBase)json.ReadObject(memstream);
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }
    }
}