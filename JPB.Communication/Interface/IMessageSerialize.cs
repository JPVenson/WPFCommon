using JPB.Communication.ComBase;

namespace JPB.Communication.Interface
{
    public interface IMessageSerialize
    {
        /// <summary>
        /// Low level Serialization for internal messages 
        /// </summary>
        /// <param name="source">the plain string</param>
        /// <returns>a valid messageobject</returns>
        TcpMessage DeSerialize(string source);
        byte[] Serialize(TcpMessage a);
    }
}