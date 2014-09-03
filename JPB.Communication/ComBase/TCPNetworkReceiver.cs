using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.ComBase
{
    public sealed class TCPNetworkReceiver : Networkbase, IDisposable
    {
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

        private readonly List<Tuple<Action<MessageBase>, Guid>> _onetimeupdated = new List<Tuple<Action<MessageBase>, Guid>>();

        private readonly List<Tuple<Action<RequstMessage>, Guid>> _pendingrequests = new List<Tuple<Action<RequstMessage>, Guid>>();

        private readonly List<Tuple<Func<RequstMessage, object>, object>> _requestHandler = new List<Tuple<Func<RequstMessage, object>, object>>();

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

        public short Port { get; private set; }

        public bool IsDisposing { get; private set; }

        public void RegisterChanged(Action<MessageBase> action, object state)
        {
            _updated.Add(new Tuple<Action<MessageBase>, object>(action, state));
        }

        public void RegisterCallback(Action<MessageBase> action, Guid guid)
        {
            _onetimeupdated.Add(new Tuple<Action<MessageBase>, Guid>(action, guid));
        }

        public void RegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            _requestHandler.Add(new Tuple<Func<RequstMessage, object>, object>(action, state));
        }

        internal void RegisterRequst(Action<RequstMessage> action, Guid guid)
        {
            _pendingrequests.Add(new Tuple<Action<RequstMessage>, Guid>(action, guid));
        }

        public void UnRegisterCallback(Guid guid)
        {
            _onetimeupdated.Remove(_onetimeupdated.FirstOrDefault(s => s.Item2 == guid));
        }

        private void MessagesOnCollectionChanged(object o,
                                                 NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action != NotifyCollectionChangedAction.Add)
                return;

            var items = notifyCollectionChangedEventArgs.NewItems.OfType<MessageBase>().ToArray();

            _workeritems.Enqueue(() =>
            {
                var item = items.First();

                if (item is RequstMessage)
                {
                    //message with return value inbound
                    var requstInbound = item as RequstMessage;
                    var firstOrDefault = _requestHandler.FirstOrDefault(pendingrequest => pendingrequest.Item2.Equals(requstInbound.InfoState));
                    if (firstOrDefault != null)
                    {
                        //Found a handler for that message and executed it
                        var result = firstOrDefault.Item1(requstInbound);

                        var sender = NetworkFactory.Instance.GetSender(Port);
                        sender.SendMessageAsync(new RequstMessage() {Message = result, ResponseFor = requstInbound.Id}, item.Sender);

                        _requestHandler.Remove(firstOrDefault);
                    }
                    else
                    {
                        //This is an awnser
                        var awnser = _pendingrequests.FirstOrDefault(pendingrequest => pendingrequest.Item2.Equals(requstInbound.ResponseFor));
                        if (awnser != null)
                            awnser.Item1(requstInbound);
                    }
                }
                else
                {
                    var updateCallbacks = _updated.Where(action => action.Item2 == null || action.Item2.Equals(item.InfoState)).ToArray();
                    foreach (
                        var action in updateCallbacks)
                        action.Item1.BeginInvoke(item, e => { }, null);

                    //Go through all one time items and check for ID
                    var oneTimeImtes = _onetimeupdated.Where(s => item.Id == s.Item2).ToArray();

                    foreach (var action in oneTimeImtes)
                    {
                        action.Item1.BeginInvoke(item, e => { }, null);
                    }

                    foreach (var useditem in oneTimeImtes)
                        _onetimeupdated.Remove(useditem);
                }
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