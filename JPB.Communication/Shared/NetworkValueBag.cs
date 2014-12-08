using System;
using System.Collections;
using System.Collections.Concurrent;
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
    public static class NetworkListControler
    {
        static NetworkListControler()
        {
            Guids = new List<string>();
        }

        internal static List<string> Guids;

        public static IEnumerable<string> GetGuids()
        {
            return Guids.ToArray();
        }
    }

    /// <summary>
    /// This class holds and Updates unsorted values that will be Synced over the Network
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetworkValueBag<T> :
        ICollection<T>,
        ICollection,
        IProducerConsumerCollection<T>,
        IList<T>,
        IList,
        INotifyCollectionChanged,
        IDisposable
    {
        public static NetworkValueBag<T> CreateNetworkValueCollection(short port, string guid)
        {
            return new NetworkValueBag<T>(port, guid);
        }

        protected static void RegisterCollecion(string guid)
        {
            if (NetworkListControler.Guids.Contains(guid))
            {
                throw new ArgumentException(@"This guid is in use. Please use a global _Uniq_ Identifier", "guid");
            }
            NetworkListControler.Guids.Add(guid);
        }

        /// <summary>
        /// Gets a non tracking version of all items that are stored on the server
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static async Task<ICollection<T>> GetCollection(string host, short port, string guid)
        {
            var sender = await NetworkFactory.Instance.GetSender(port).SendRequstMessage<T[]>(new RequstMessage
            {
                InfoState = NetworkCollectionProtocol.CollectionGetCollection,
                Message = guid
            }, host);

            return sender;
        }

        public string ConnectedToHost { get; protected set; }

        public List<string> CollectionRecievers { get; protected set; }

        protected ICollection<T> LocalValues { get; set; }
        protected readonly TCPNetworkReceiver TcpNetworkReceiver;
        protected readonly TCPNetworkSender TcpNetworkSernder;

        protected NetworkValueBag(short port, string guid)
        {
            RegisterCollecion(guid);

            //objects that Impliments or Contains a Serializable Attribute are supported
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
            var collection = GetCollection(host, this.Port, Guid);

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
            if (arg.Message != Guid)
                return null;

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
        public bool IsFixedSize { get; private set; }
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
                array = ToArray();
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

        public int Add(object value)
        {
            lock (SyncRoot)
            {
                this.Add((T)value);
                return Count;
            }
        }

        /// <summary>
        /// Not thread save
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(object value)
        {
            return this.Contains((T)value);
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

        /// <summary>
        /// Not thread save
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            this.Remove((T)value);
        }

        public int IndexOf(T item)
        {
            int result = -1;
            for (int i = 0; i < LocalValues.Count; i++)
            {
                var localValue = this[i];

                if (localValue.Equals(item))
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// To be Supported
        /// throw new NotImplementedException();
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// To be Supported
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            this.Remove(this[index]);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        public T this[int index]
        {
            get { return this.ElementAt(index); }
            set { this.Insert(index, value); }
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

        public bool TryAdd(T item)
        {
            lock (SyncRoot)
            {
                this.Add(item);
                return this.Contains(item);
            }
        }

        public bool TryTake(out T item)
        {
            lock (SyncRoot)
            {
                item = this[this.Count];
            }
            return true;
        }

        public T[] ToArray()
        {
            lock (SyncRoot)
            {
                return LocalValues.ToArray();
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

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void TriggerAdd(T added)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
        }

        protected virtual void TriggerRemove(T removed, int result)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, result));
        }

        protected virtual void TriggerReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, e);
        }

        public virtual async Task<bool> Reload()
        {
            var collection = await GetCollection(ConnectedToHost, 1337, Guid);
            lock (SyncRoot)
            {
                LocalValues.Clear();

                if (collection == null)
                    return false;

                foreach (var item in collection)
                {
                    LocalValues.Add(item);
                }

                return true;
            }
        }

        public void Dispose()
        {
            if (IsDisposing)
                return;

            IsDisposing = true;

            PushUnRegisterMessage();
            UnRegisterCallbacks();

            NetworkListControler.Guids.Remove(Guid);
            LocalValues = null;
            CollectionRecievers = null;
        }

        ~NetworkValueBag()
        {
            Dispose();
        }
    }
}
