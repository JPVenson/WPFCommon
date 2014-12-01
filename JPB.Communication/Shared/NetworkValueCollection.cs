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
    /// This class holds and Updates unsorted values that will be Synced over the Network
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetworkValueCollection<T> :
        ICollection<T>,
        ICollection,
        INotifyCollectionChanged,
        IDisposable
    {
        public static NetworkValueCollection<T> CreateNetworkValueCollection(short port, string guid)
        {
            return new NetworkValueCollection<T>(port, guid);
        }

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

        public string ConnectedToHost { get; protected set; }
        protected static List<string> Guids;
        public List<string> CollectionRecievers { get; protected set; }

        protected ICollection<T> LocalValues;
        protected readonly TCPNetworkReceiver TcpNetworkReceiver;
        protected readonly TCPNetworkSender TcpNetworkSernder;

        protected NetworkValueCollection(short port, string guid)
        {
            RegisterCollecion(guid);

            //if (!typeof(T).IsValueType && !typeof(T).IsPrimitive)
            //{
            //    throw new TypeLoadException("Typeof T must be a Value type ... please use the NonValue collection");
            //}

            CollectionRecievers = new List<string>();

            Port = port;
            Guid = guid;
            LocalValues = new ObservableCollection<T>();
            SyncRoot = new object();


            TcpNetworkReceiver = NetworkFactory.Instance.GetReceiver(port);
            TcpNetworkSernder = NetworkFactory.Instance.GetSender(port);
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            TcpNetworkReceiver.RegisterChanged(pPullAddMessage, NetworkCollectionProtocol.CollectionAdd);
            TcpNetworkReceiver.RegisterChanged(pPullClearMessage, NetworkCollectionProtocol.CollectionReset);
            TcpNetworkReceiver.RegisterChanged(pPullRemoveMessage, NetworkCollectionProtocol.CollectionRemove);
            TcpNetworkReceiver.RegisterChanged(PullRegisterMessage, NetworkCollectionProtocol.CollectionRegisterUser);
            TcpNetworkReceiver.RegisterChanged(PullUnRegisterMessage, NetworkCollectionProtocol.CollectionUnRegisterUser);
            TcpNetworkReceiver.RegisterRequstHandler(PullGetCollectionMessage, NetworkCollectionProtocol.CollectionGetCollection);
            TcpNetworkReceiver.RegisterRequstHandler(PullConnectMessage, NetworkCollectionProtocol.CollectionGetUsers);
        }

        private void UnRegisterCallbacks()
        {
            TcpNetworkReceiver.UnregisterChanged(pPullAddMessage, NetworkCollectionProtocol.CollectionAdd);
            TcpNetworkReceiver.UnregisterChanged(pPullClearMessage, NetworkCollectionProtocol.CollectionReset);
            TcpNetworkReceiver.UnregisterChanged(pPullRemoveMessage, NetworkCollectionProtocol.CollectionRemove);
            TcpNetworkReceiver.UnregisterChanged(PullRegisterMessage, NetworkCollectionProtocol.CollectionRegisterUser);
            TcpNetworkReceiver.UnregisterChanged(PullUnRegisterMessage, NetworkCollectionProtocol.CollectionUnRegisterUser);
            TcpNetworkReceiver.UnRegisterRequstHandler(PullGetCollectionMessage, NetworkCollectionProtocol.CollectionGetCollection);
            TcpNetworkReceiver.UnRegisterRequstHandler(PullConnectMessage, NetworkCollectionProtocol.CollectionGetUsers);
        }

        protected object PullConnectMessage(RequstMessage arg)
        {
            if (arg.Message is string && arg.Message == Guid)
                lock (SyncRoot)
                {
                    if (!this.CollectionRecievers.Contains(arg.Sender))
                    {
                        this.CollectionRecievers.Add(arg.Sender);
                    }
                    if (!this.CollectionRecievers.Contains(NetworkInfoBase.IpAddress.ToString()))
                    {
                        this.CollectionRecievers.Add(NetworkInfoBase.IpAddress.ToString());
                    }
                    return this.CollectionRecievers.ToArray();
                }
            return null;
        }

        /// <summary>
        /// Connects this Collection to one Host inside the NetworkCollection
        /// After the Connect this instance will get:
        /// All Other existing Users
        /// A Copy of the Current Network List
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public virtual async Task<bool> Connect(string host)
        {
            var collection = GetCollection(host, this.Port);

            ConnectedToHost = host;
            var sendRequstMessage = await TcpNetworkSernder.SendRequstMessage<string[]>(
                new RequstMessage() { InfoState = NetworkCollectionProtocol.CollectionGetUsers, Message = Guid }, host);

            var users = sendRequstMessage;

            if (users == null)
            {
                return false;
            }

            this.CollectionRecievers = new List<string>(users);
#pragma warning disable 4014
            TcpNetworkSernder.SendMultiMessageAsync(
#pragma warning restore 4014
new MessageBase() { InfoState = NetworkCollectionProtocol.CollectionRegisterUser }, users);
            Registerd = true;

            foreach (var item in await collection)
            {
                LocalValues.Add(item);
                TriggerAdd(item);
            }
            return true;
        }

        protected object PullGetCollectionMessage(RequstMessage arg)
        {
            lock (SyncRoot)
            {
                return LocalValues.ToArray();
            }
        }

        public short Port { get; private set; }
        public string Guid { get; protected set; }
        public int Count { get { return LocalValues.Count; } }
        public object SyncRoot { get; protected set; }
        public bool IsReadOnly { get { return false; } }
        public bool IsDisposing { get; protected set; }

        public bool Registerd { get; protected set; }

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
                //We got a Single 
                if (obj.Message is IEnumerable<string>)
                {
                    var diabled = obj.Message as IEnumerable<string>;
                    CollectionRecievers.RemoveAll(s => diabled.Contains(s));
                }
                else
                {
                    CollectionRecievers.RemoveAll(s => s == obj.Sender);
                }
            }
        }

        protected void PushUnRegisterMessage()
        {
            lock (SyncRoot)
            {
                Registerd = false;
                TcpNetworkSernder.SendMultiMessageAsync(new MessageBase(new object()), CollectionRecievers.ToArray());
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
                        LocalValues.Add((T)mess.Value);
                        TriggerAdd((T)mess.Value);
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
                        LocalValues.Clear();
                        TriggerReset();
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
                        var value = (T)mess.Value;
                        var indexOf = IndexOf(value);
                        var remove = LocalValues.Remove(value);
                        if (remove)
                            TriggerRemove(value, indexOf);
                    }
                }
            }
        }

        protected async void SendPessage(string id, object value)
        {
            var mess = new MessageBase(new NetworkCollectionMessage(value)
            {
                Guid = Guid
            })
            {
                InfoState = id
            };

            string[] ips;
            lock (SyncRoot)
            {
                ips = CollectionRecievers.Where(s => !s.Equals(NetworkInfoBase.IpAddress.ToString())).ToArray();
            }

            //Possible long term work
            var sendMultiMessage = await TcpNetworkSernder.SendMultiMessageAsync(mess, ips);

            lock (SyncRoot)
            {
                CollectionRecievers.RemoveAll(sendMultiMessage.Contains);
            }
        }

        protected async void PushAddMessage(T item)
        {
            SendPessage(NetworkCollectionProtocol.CollectionAdd, item);
        }

        protected async void PushClearMessage()
        {
            SendPessage(NetworkCollectionProtocol.CollectionReset, default(T));
        }

        protected async void PushRemoveMessage(T item)
        {
            SendPessage(NetworkCollectionProtocol.CollectionRemove, item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            T[] array;
            lock (SyncRoot)
            {
                array = LocalValues.ToArray();
            }
            return array.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(T item)
        {
            lock (SyncRoot)
            {
                PushAddMessage(item);
                LocalValues.Add(item);
                TriggerAdd(item);
            }
        }

        public void Clear()
        {
            lock (SyncRoot)
            {
                PushClearMessage();
                LocalValues.Clear();
                TriggerReset();
            }
        }

        public int IndexOf(T item)
        {
            int result = -1;
            for (int i = 0; i < LocalValues.Count; i++)
            {
                var localValue = LocalValues.ElementAt(i);

                if (localValue.Equals(item))
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        public bool Remove(T item)
        {
            lock (SyncRoot)
            {
                PushRemoveMessage(item);
                var remove = LocalValues.Remove(item);
                if (remove)
                    TriggerRemove(item, IndexOf(item));
                return remove;
            }
        }

        public bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return LocalValues.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                LocalValues.CopyTo(array, arrayIndex);
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                for (int i = index; i < LocalValues.Count; i++)
                {
                    var localValue = LocalValues.ElementAt(i);
                    array.SetValue(localValue, i);
                }
            }
        }

        public void Dispose()
        {
            if (IsDisposing)
                return;

            IsDisposing = true;
            UnRegisterCallbacks();

            Guids.Remove(Guid);
            LocalValues = null;
            PushUnRegisterMessage();
        }

        ~NetworkValueCollection()
        {
            Dispose();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void TriggerAdd(T added)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
        }

        public void TriggerRemove(T removed, int result)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, result));
        }

        public void TriggerReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        public virtual async void Reload()
        {
            var collection = await GetCollection(ConnectedToHost, 1337);
            lock (SyncRoot)
            {
                LocalValues.Clear();
                foreach (var item in collection)
                {
                    LocalValues.Add(item);
                }
            }
        }
    }
}
