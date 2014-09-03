using System.Collections.ObjectModel;
using System.Text;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    public abstract class ConnectionBase : Networkbase
    {
        public static Encoding Encoding = Encoding.UTF8;

        protected ConnectionBase()
        {
            Messages = new ObservableCollection<MessageBase>();
        }

        public TcpMessage LastMessage { get; set; }

        public ObservableCollection<MessageBase> Messages { get; set; }

        public bool Parse(string strReceived)
        {
            //Console.WriteLine("RECIVED: \"" + strReceived + "\"\r\n");
            TcpMessage item = DeSerialize(strReceived);

            if (item != null)
            {
                LastMessage = item;
                Messages.Add(base.LoadMessageBaseFromBinary(item.MessageBase));
                return true;
            }
            return false;
        }
    }
}