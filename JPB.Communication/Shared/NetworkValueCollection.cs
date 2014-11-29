using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.Shared
{
    /// <summary>
    /// This class holds and Updates values that will be Synced over the Network
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetworkValueCollection<T> :
        ICollection<T>,
        ICollection,
        INotifyCollectionChanged,
        IDisposable
    {
        static NetworkValueCollection()
        {
            Guids = new List<string>();
        }

        protected static void RegisterCollecion(string guid)
        {
            if (Guids.Contains(guid))
            {
                throw new ArgumentException(@"This guid is in use. Please use a global _Uniq_ Identifier", "guid");
            }
        }

        public static async Task<ICollection<T>> GetCollection(string host, short port)
        {
            var sender = await NetworkFactory.Instance.GetSender(port).SendRequstMessage<T[]>(new RequstMessage
            {
                InfoState = NetworkCollectionProtocol.CollectionGetCollection
            }, host);

            return sender;
        }

        public string ConnectedToHost { get; private set; }
        protected static List<string> Guids;
        public List<string> CollectionRecievers { get; protected set; }

        protected ObservableCollection<T> _localValues;
        readonly TCPNetworkSender _tcpNetworkSernder;

        public NetworkValueCollection(short port, string guid)
        {
            RegisterCollecion(guid);

            //if (!typeof(T).IsValueType && !typeof(T).IsPrimitive)
            //{
            //    throw new TypeLoadException("Typeof T must be a Value type ... please use the NonValue collection");
            //}

            CollectionRecievers = new List<string>();

            Port = port;
            Guid = guid;
            _localValues = new ObservableCollection<T>();
            SyncRoot = new object();
            TCPNetworkReceiver tcpNetworkReceiver = NetworkFactory.Instance.GetReceiver(port);
            tcpNetworkReceiver.RegisterChanged(pPullAddMessage, NetworkCollectionProtocol.CollectionAdd);
            tcpNetworkReceiver.RegisterChanged(pPullClearMessage, NetworkCollectionProtocol.CollectionReset);
            tcpNetworkReceiver.RegisterChanged(pPullRemoveMessage, NetworkCollectionProtocol.CollectionRemove);
            tcpNetworkReceiver.RegisterChanged(PullRegisterMessage, NetworkCollectionProtocol.CollectionRegisterUser);
            tcpNetworkReceiver.RegisterChanged(PullUnRegisterMessage, NetworkCollectionProtocol.CollectionUnRegisterUser);
            tcpNetworkReceiver.RegisterRequstHandler(PullGetCollectionMessage, NetworkCollectionProtocol.CollectionGetCollection);
            tcpNetworkReceiver.RegisterRequstHandler(PullConnectMessage, NetworkCollectionProtocol.CollectionGetUsers);

            _tcpNetworkSernder = NetworkFactory.Instance.GetSender(port);
        }

        protected object PullConnectMessage(RequstMessage arg)
        {
            lock (SyncRoot)
            {
                if (!this.CollectionRecievers.Contains(arg.Sender))
                    this.CollectionRecievers.Add(arg.Sender);
                if (!this.CollectionRecievers.Contains(NetworkInfoBase.IpAddress.ToString()))
                    this.CollectionRecievers.Add(NetworkInfoBase.IpAddress.ToString());
                return this.CollectionRecievers.ToArray();
            }
        }

        /// <summary>
        /// Connects this Collection to one Host inside the NetworkCollection
        /// After the Connect this instance will get:
        /// All Other existing Users
        /// A Copy of the Current Network List
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public async Task<bool> Connect(string host)
        {
            ConnectedToHost = host;
            var sendRequstMessage = await _tcpNetworkSernder.SendRequstMessage<string[]>(
                new RequstMessage() { InfoState = NetworkCollectionProtocol.CollectionGetUsers }, host);

            var users = sendRequstMessage;

            if (users == null)
            {
                return false;
            }

            this.CollectionRecievers = new List<string>(users);
#pragma warning disable 4014
            _tcpNetworkSernder.SendMultiMessageAsync(
#pragma warning restore 4014
new MessageBase() { InfoState = NetworkCollectionProtocol.CollectionRegisterUser }, users);

            foreach (var item in await GetCollection(host, this.Port))
            {
                _localValues.Add(item);
            }
            return true;
        }

        protected object PullGetCollectionMessage(RequstMessage arg)
        {
            lock (SyncRoot)
            {
                return _localValues.ToArray();
            }
        }

        public short Port { get; private set; }
        public string Guid { get; protected set; }
        public int Count { get { return _localValues.Count; } }
        public object SyncRoot { get; protected set; }
        public bool IsReadOnly { get { return false; } }
        public bool IsDisposing { get; protected set; }
        public bool IsSynchronized
        {
            get { return !Monitor.IsEntered(SyncRoot); }
        }

        protected void PullRegisterMessage(MessageBase obj)
        {
            lock (SyncRoot)
            {
                if (!CollectionRecievers.Contains(obj.Sender))
                    CollectionRecievers.Add(obj.Sender);
            }
        }

        protected void PullUnRegisterMessage(MessageBase obj)
        {
            lock (SyncRoot)
            {
                CollectionRecievers.RemoveAll(s => s.Equals(obj.Sender));
            }
        }

        private void pPullAddMessage(MessageBase obj)
        {
            PullAddMessage(obj);
        }

        private void pPullClearMessage(MessageBase obj)
        {
            PullClearMessage(obj);
        }

        private void pPullRemoveMessage(MessageBase obj)
        {
            PullRemoveMessage(obj);
        }

        protected virtual void PullAddMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
                {
                    lock (SyncRoot)
                    {
                        _localValues.Add((T)mess.Value);
                    }
                }
            }
        }

        protected virtual void PullClearMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid))
                {
                    lock (SyncRoot)
                    {
                        _localValues.Clear();
                    }
                }
            }
        }

        protected virtual void PullRemoveMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
                {
                    lock (SyncRoot)
                    {
                        _localValues.Remove((T)mess.Value);
                    }
                }
            }
        }

        protected void SendPessage(string id, T value)
        {
            var mess = new MessageBase(new NetworkCollectionMessage(value) { Guid = Guid }) { InfoState = id };

            string[] ips;
            lock (SyncRoot)
            {
                ips = CollectionRecievers.Where(s => !s.Equals(NetworkInfoBase.IpAddress.ToString())).ToArray();
            }

            //Possible long term work
            var sendMultiMessage = _tcpNetworkSernder.SendMultiMessage(mess, ips);

            lock (SyncRoot)
            {
                CollectionRecievers.RemoveAll(sendMultiMessage.Contains);
            }
        }

        protected void PushAddMessage(T item)
        {
            SendPessage(NetworkCollectionProtocol.CollectionAdd, item);
        }

        protected void PushClearMessage()
        {
            SendPessage(NetworkCollectionProtocol.CollectionReset, default(T));
        }

        protected void PushRemoveMessage(T item)
        {
            SendPessage(NetworkCollectionProtocol.CollectionRemove, item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            T[] array;
            lock (SyncRoot)
            {
                array = _localValues.ToArray();
            }
            return array.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            lock (SyncRoot)
            {
                PushAddMessage(item);
                _localValues.Add(item);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                PushClearMessage();
                _localValues.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return _localValues.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                _localValues.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                PushRemoveMessage(item);
                return _localValues.Remove(item);
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                for (int i = index; i < _localValues.Count; i++)
                {
                    var localValue = _localValues[i];
                    array.SetValue(localValue, i);
                }
            }
        }

        public void Dispose()
        {
            if (IsDisposing)
                return;

            IsDisposing = true;
            Guids.Remove(Guid);
            _localValues = null;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { this._localValues.CollectionChanged += value; }
            remove { this._localValues.CollectionChanged -= value; }
        }

        public virtual async void Reload()
        {
            var collection = await GetCollection(ConnectedToHost, 1337);
            lock (SyncRoot)
            {
                foreach (var item in collection)
                {
                    _localValues.Add(item);
                }
            }
        }
    }
}
