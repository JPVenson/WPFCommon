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
            OnNewItemLoadedSuccess += TcpConnectionOnOnNewItemLoadedSuccess;
            Port = port;
            _sock = new Socket(IPAddress.Any.AddressFamily,
                               SocketType.Stream,
                               ProtocolType.Tcp);
            _sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); 
            _sock.Bind(new IPEndPoint(NetworkInfoBase.IpAddress, Port));

            // Bind the socket to the address and port.
            _sock.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.MaxConnections);
            // Start listening.
            _sock.Listen(5000);
            // Set up the callback to be notified when somebody requests
            // a new connection.
            _sock.BeginAccept(OnConnectRequest, _sock);
        }

        private void TcpConnectionOnOnNewItemLoadedSuccess(MessageBase mess, short port)
        {
            if (port == Port)
            {
                var messCopy = mess;
                _workeritems.Enqueue(() =>
                {
                    if (messCopy is RequstMessage)
                    {
                        //message with return value inbound
                        var requstInbound = messCopy as RequstMessage;
                        var firstOrDefault = _requestHandler.Where(pendingrequest => pendingrequest.Item2.Equals(requstInbound.InfoState)).ToArray();
                        if (firstOrDefault.Any())
                        {
                            object result = null;

                            foreach (var tuple in firstOrDefault)
                            {
                                //Found a handler for that message and executed it

                                result = tuple.Item1(requstInbound);
                                if (result == null)
                                    continue;
                            }

                            if(result == null)
                                return;

                            var sender = NetworkFactory.Instance.GetSender(Port);
                            sender.SendMessageAsync(new RequstMessage()
                            {
                                Message = result,
                                ResponseFor = requstInbound.Id
                            }, messCopy.Sender);
                        }
                        else
                        {
                            //This is an awnser
                            var awnser = _pendingrequests.FirstOrDefault(pendingrequest => pendingrequest.Item2.Equals(requstInbound.ResponseFor));
                            if (awnser != null)
                                awnser.Item1(requstInbound);
                            _pendingrequests.Remove(awnser);
                        }
                    }
                    else
                    {
                        var updateCallbacks = _updated.Where(action => action.Item2 == null || action.Item2.Equals(messCopy.InfoState)).ToArray();
                        foreach (
                            var action in updateCallbacks)
                            action.Item1.BeginInvoke(messCopy, e => { }, null);

                        //Go through all one time items and check for ID
                        var oneTimeImtes = _onetimeupdated.Where(s => messCopy.Id == s.Item2).ToArray();

                        foreach (var action in oneTimeImtes)
                        {
                            action.Item1.BeginInvoke(messCopy, e => { }, null);
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

        public bool IsDisposing { get; private set; }

        public void UnregisterChanged(Action<MessageBase> action, object state)
        {
            var enumerable = _updated.FirstOrDefault(s => s.Item1 == action && s.Item2 == state);
            if (enumerable != null)
            {
                _updated.Remove(enumerable);
            }
        }

        public void UnregisterChanged(Action<MessageBase> action)
        {
            var enumerable = _updated.FirstOrDefault(s => s.Item1 == action);
            if (enumerable != null)
            {
                _updated.Remove(enumerable);
            }
        }

        /// <summary>
        /// Register a Callback localy that will be used when a new message is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="state">Maybe an Enum?</param>
        public void RegisterChanged(Action<MessageBase> action, object state)
        {
            _updated.Add(new Tuple<Action<MessageBase>, object>(action, state));
        }

        /// <summary>
        /// Register a Callback localy that will be used when a message contains a given Guid
        /// </summary>
        /// <param name="action"></param>
        /// <param name="guid"></param>
        public void RegisterCallback(Action<MessageBase> action, Guid guid)
        {
            _onetimeupdated.Add(new Tuple<Action<MessageBase>, Guid>(action, guid));
        }

        /// <summary>
        /// Register a Callback localy that will be used when a Requst is inbound that has state in its InfoState
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        public void RegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            _requestHandler.Add(new Tuple<Func<RequstMessage, object>, object>(action, state));
        }


        public void UnRegisterRequstHandler(Func<RequstMessage, object> action, object state)
        {
            var enumerable = _requestHandler.FirstOrDefault(s => s.Item1 == action && state == s.Item2);
            if (enumerable != null)
            {
                _requestHandler.Remove(enumerable);
            }
        }

        public void UnRegisterRequstHandler(Func<RequstMessage, object> action)
        {
            var enumerable = _requestHandler.FirstOrDefault(s => s.Item1 == action);
            if (enumerable != null)
            {
                _requestHandler.Remove(enumerable);
            }
        }

        internal void RegisterRequst(Action<RequstMessage> action, Guid guid)
        {
            _pendingrequests.Add(new Tuple<Action<RequstMessage>, Guid>(action, guid));
        }

        internal void UnRegisterRequst(Guid guid)
        {
            var firstOrDefault = _pendingrequests.FirstOrDefault(s => s.Item2 == guid);
            if (firstOrDefault != null)
            {
                _pendingrequests.Remove(firstOrDefault);
            }
        }

        internal void UnRegisterCallback(Guid guid)
        {
            _onetimeupdated.Remove(_onetimeupdated.FirstOrDefault(s => s.Item2 == guid));
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

        internal void OnConnectRequest(IAsyncResult result)
        {
            // Get the socket (which should be this listener's socket) from
            // the argument.
            var sock = ((Socket)result.AsyncState);

            // Create a new client connection, using the primary socket to
            // spawn a new socket.
            new TcpConnection(sock.EndAccept(result)) { Port = Port };
            // Tell the listener socket to start listening again.
            _sock.BeginAccept(OnConnectRequest, sock);
        }

        public override short Port { get; internal set; }
    }
}