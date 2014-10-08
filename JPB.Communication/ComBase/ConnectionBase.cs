using System;
using System.Collections.ObjectModel;
using System.Text;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    internal abstract class ConnectionBase : Networkbase
    {
        internal static Encoding Encoding = Encoding.UTF8;

        protected ConnectionBase()
        {
            Messages = new ObservableCollection<MessageBase>();
        }

        internal TcpMessage LastMessage { get; set; }

        internal ObservableCollection<MessageBase> Messages { get; set; }

        internal bool Parse(string strReceived)
        {
            //Console.WriteLine("RECIVED: \"" + strReceived + "\"\r\n");
            TcpMessage item;
            try
            {
                item = DeSerialize(strReceived);

                if (item != null)
                {
                    LastMessage = item;
                    Messages.Add(base.LoadMessageBaseFromBinary(item.MessageBase));
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}