using System;
using System.Collections.ObjectModel;
using System.Text;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    internal abstract class ConnectionBase : Networkbase
    {
        protected ConnectionBase()
        {

        }
        
        internal bool Parse(string strReceived)
        {
            //Console.WriteLine("RECIVED: \"" + strReceived + "\"\r\n");
            TcpMessage item;
            try
            {
                RaiseIncommingMessage(strReceived);
                item = DeSerialize(strReceived);

                if (item != null)
                {
                    var loadMessageBaseFromBinary = base.LoadMessageBaseFromBinary(item.MessageBase);
                    RaiseNewItemLoadedSuccess(loadMessageBaseFromBinary);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                RaiseNewItemLoadedFail(strReceived);
                return false;
            }
        }
    }
}