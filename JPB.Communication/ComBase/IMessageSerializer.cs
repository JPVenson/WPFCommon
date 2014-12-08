using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    public interface IMessageSerializer
    {
        /// <summary>
        /// Is used to convert the Complete Message Object into a byte[] that will be transferted to the remote computer
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        byte[] SerializeMessage(TcpMessage a);
        /// <summary>
        /// Is used to convert the message object that is a Property of the TCP message into an other format then the TCP message 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        byte[] SerializeMessageContent(MessageBase A);

        /// <summary>
        /// Converts the output from the TCP network adapter into a valid TCP message
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        TcpMessage DeSerializeMessage(byte[] source);

        /// <summary>
        /// Converts the content of an TCP message into an object that will be deliverd to the Components
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        MessageBase DeSerializeMessageContent(byte[] source);

        /// <summary>
        /// tries to convert the Message to a string Representation
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string ResolveStringContent(byte[] message);
    }
}