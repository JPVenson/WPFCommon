using System.IO;
using System.IO.Compression;
using System.Text;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    public class BinaryCompressedMessageSerilalizer : IMessageSerializer
    {
        static BinaryCompressedMessageSerilalizer()
        {
            DefaultMessageSerlilizer = new DefaultMessageSerlilizer();
        }

        public static DefaultMessageSerlilizer DefaultMessageSerlilizer { get; set; }

        public byte[] SerializeMessage(TcpMessage a)
        {
            var mess = DefaultMessageSerlilizer.SerializeMessage(a);
            return Compress(mess);
        }

        public byte[] SerializeMessageContent(MessageBase A)
        {
            return DefaultMessageSerlilizer.SerializeMessageContent(A);
        }

        public TcpMessage DeSerializeMessage(byte[] source)
        {
            source = DeCompress(source);
            return DefaultMessageSerlilizer.DeSerializeMessage(source);
        }

        public MessageBase DeSerializeMessageContent(byte[] source)
        {
            return DefaultMessageSerlilizer.DeSerializeMessageContent(source);
        }

        public string ResolveStringContent(byte[] message)
        {
            return Encoding.ASCII.GetString(message);
        }

        /// <summary>
        /// Compresses byte array to new byte array.
        /// </summary>
        public static byte[] Compress(byte[] raw)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        /// <summary>
        /// UnCompresses byte array to new byte array.
        /// </summary>
        public static byte[] DeCompress(byte[] raw)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Decompress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
    }
}