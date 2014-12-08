using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace JPB.Communication.ComBase
{
    internal class TcpConnection : ConnectionBase, IDisposable
    {
        private readonly Socket sock;
        private InternalMemoryHolder datarec;
        // Pick whatever encoding works best for you.  Just make sure the remote 
        // host is using the same encoding.

        internal TcpConnection(Socket s)
        {
            datarec = new InternalMemoryHolder();
            sock = s;
            datarec.Add(new byte[sock.ReceiveBufferSize]);
            // Start listening for incoming data.  (If you want a multi-
            // threaded service, you can start this method up in a separate
            // thread.)
            BeginReceive();
        }

        // Call this method to set this connection's socket up to receive data.
        private void BeginReceive()
        {
            var last = datarec.Last;
            sock.BeginReceive(
                last, 0,
                last.Length,
                SocketFlags.None,
                OnBytesReceived,
                this);
        }


        // This is the method that is called whenever the socket receives
        // incoming bytes.
        protected void OnBytesReceived(IAsyncResult result)
        {
            // End the data receiving that the socket has done and get
            // the number of bytes read.
            int rec;
            try
            {
                rec = sock.EndReceive(result);
            }
            catch (Exception)
            {
                Dispose();
                return;
            }

            //to incomming data left
            //try to concat the message
            if (rec == 0)
            {
                var buff = NullRemover(datarec.Get());
                int count = buff.Count();
                var compltearray = new byte[count];
                for (int i = 0; i < count; i++)
                    compltearray.SetValue(buff[i], i);

                Parse(compltearray);
                return;
            }
            
            var newbuff = new byte[sock.ReceiveBufferSize];
            datarec.Add(newbuff);

            sock.BeginReceive(
                newbuff, 0,
                newbuff.Length,
                SocketFlags.None,
                OnBytesReceived,
                this);

        }

        private byte[] NullRemover(byte[] dataStream)
        {
            int i;
            var temp = new List<byte>();
            for (i = 0; i < dataStream.Count() - 1; i++)
            {
                if (dataStream[i] == 0x00) continue;
                temp.Add(dataStream[i]);
            }
            return temp.ToArray();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            datarec.Dispose();
        }

        #endregion

        public override short Port { get; internal set; }
    }
}