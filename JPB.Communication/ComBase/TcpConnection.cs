using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase
{
    internal class InternalMemoryHolder : IDisposable
    {
        private List<byte[]> _datarec = new List<byte[]>();

        internal byte[] Last { get; set; }

        internal bool IsSharedMem { get; set; }

        private FileStream _fileStream;

        private Task _writeAsync;

        internal async void Add(byte[] bytes)
        {
            Last = bytes;

            if (_writeAsync != null)
                await _writeAsync;

            if (_datarec.Count >= 10 && !IsSharedMem)
            {
                _fileStream = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete);
                var completeBytes = privateGet();
                _writeAsync = _fileStream.WriteAsync(completeBytes, 0, completeBytes.Length);
                IsSharedMem = true;
            }
            if (IsSharedMem)
            {
                _writeAsync = _fileStream.WriteAsync(bytes, 0, bytes.Length);
            }
            else
            {
                _datarec.Add(bytes);
            }
        }

        private byte[] privateGet()
        {
            return !IsSharedMem ? _datarec.SelectMany(s => s).ToArray() : Last;
        }

        internal byte[] Get()
        {
            return privateGet();
        }

        public void Dispose()
        {
            if (_fileStream != null)
            {
                try
                {
                    if (File.Exists(_fileStream.Name))
                        File.Delete(_fileStream.Name);
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    _fileStream.Dispose();
                }
            }
            _datarec = null;
        }
    }


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
                return;
            }

            if (rec == 0)
            {
                var buff = NullRemover(datarec.Get());
                int count = buff.Count();
                var compltearray = new byte[count];
                for (int i = 0; i < count; i++)
                    compltearray.SetValue(buff[i], i);

                string strfull = (Encoding.GetString(compltearray, 0, count));

                Parse(strfull);
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