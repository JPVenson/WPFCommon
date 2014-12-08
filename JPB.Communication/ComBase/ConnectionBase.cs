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

        public const string ErrorDueParse = "ERR / Message is Corrupt";
        
        internal bool Parse(byte[] received)
        {
            TcpMessage item;
            try
            {
                item = DeSerialize(received);
                RaiseIncommingMessage(item);

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
                string source;
                try
                {
                    source = this.Serlilizer.ResolveStringContent(received);
                }
                catch (Exception)
                {
                    source = ErrorDueParse;
                }

                RaiseNewItemLoadedFail(source);
                return false;
            }
        }
    }
}