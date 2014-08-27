using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JPB.Communication.ComBase
{
    public sealed class TCPNetworkReceiver : Networkbase, IDisposable
    {
        private readonly List<Tuple<Action<MessageBase>, Guid>> _onetimeupdated =
            new List<Tuple<Action<MessageBase>, Guid>>();

        private readonly Socket _sock;

        private readonly List<Tuple<Action<MessageBase>, object>> _updated =
            new List<Tuple<Action<MessageBase>, object>>();

        private readonly ConcurrentQueue<Action> _workeritems = new ConcurrentQueue<Action>();
        private AutoResetEvent _autoResetEvent;

        private bool _isWorking;

        #region Implementation of IDisposable

        public void Dispose()
        {
            IsDisposing = true;
            if (_autoResetEvent != null)
                _autoResetEvent.WaitOne();
            _sock.Dispose();
        }

        #endregion

        internal TCPNetworkReceiver(short port)
        {
            Port = port;
            //_server = new TcpListener(NetworkInfoBase.IpAddress, NetworkInfoBase.Port);
            //_server.Start();
            //var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            //var activeTcpListeners = ipGlobalProperties.GetActiveTcpListeners();
            //var any = activeTcpListeners.Any(s => s.Port == port);

            //if(any)
            //    throw new NotSupportedException("The port is in use");

            _sock = new Socket(IPAddress.Any.AddressFamily,
                               SocketType.Stream,
                               ProtocolType.Tcp);
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _sock.Bind(new IPEndPoint(NetworkInfoBase.IpAddress, Port));

            // Bind the socket to the address and port.
            
            // Start listening.
            _sock.Listen(5000);
            // Set up the callback to be notified when somebody requests
            // a new connection.
            _sock.BeginAccept(OnConnectRequest, _sock);
        }

        public short Port { get; private set; }

        public bool IsDisposing { get; set; }

        public void RegisterChanged(Action<MessageBase> action, object state)
        {
            _updated.Add(new Tuple<Action<MessageBase>, object>(action, state));
        }

        public void RegisterCallback(Action<MessageBase> action, Guid guid)
        {
            _onetimeupdated.Add(new Tuple<Action<MessageBase>, Guid>(action, guid));
        }

        public void UnRegisterCallback(Guid guid)
        {
            _onetimeupdated.Remove(_onetimeupdated.FirstOrDefault(s => s.Item2 == guid));
        }

        private void MessagesOnCollectionChanged(object sender,
                                                 NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action != NotifyCollectionChangedAction.Add)
                return;

            var items = notifyCollectionChangedEventArgs.NewItems.OfType<MessageBase>().ToArray();

            _workeritems.Enqueue(() =>
            {
                var item = items.First();
                var updateCallbacks = _updated.Where(action => action.Item2 == null || action.Item2.Equals(item.InfoState)).ToArray();
                foreach (
                    var action in updateCallbacks)
                    action.Item1.BeginInvoke(item, e => { }, null);

                var useditems = new List<Tuple<Action<MessageBase>, Guid>>();

                foreach (
                    var action in
                        _onetimeupdated.Where(s => item.Id == s.Item2))
                {
                    action.Item1.Invoke(item);
                    useditems.Add(action);
                }

                foreach (var useditem in useditems)
                    _onetimeupdated.Remove(useditem);
            });
            if (_isWorking)
                return;

            _isWorking = true;
            var task = new Task(WorkOnItems);
            task.Start();
            task.ContinueWith(s => { _isWorking = false; });
        }

        private void WorkOnItems()
        {
            _autoResetEvent = new AutoResetEvent(false);
            while (_workeritems.Any())
            {
                if (IsDisposing)
                    break;

                Action action = null;
                if (!_workeritems.TryDequeue(out action))
                    return;
                action();
            }
            _autoResetEvent.Set();
        }

        // This is the method that is called when the socket recives a request
        // for a new connection.
        public void OnConnectRequest(IAsyncResult result)
        {
            // Get the socket (which should be this listener's socket) from
            // the argument.
            var sock = ((Socket)result.AsyncState);

            // Create a new client connection, using the primary socket to
            // spawn a new socket.
            var newConn = new TcpConnection(sock.EndAccept(result));
            newConn.Messages.CollectionChanged -= MessagesOnCollectionChanged;
            newConn.Messages.CollectionChanged += MessagesOnCollectionChanged;
            // Tell the listener socket to start listening again.
            _sock.BeginAccept(OnConnectRequest, sock);
        }
    }
}