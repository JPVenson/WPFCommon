using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Threading;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Shared;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.WPFNetworkIntigration
{
    /// <summary>
    /// This class holds and Updates values that will be Synced over the Network
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableNetworkValueCollection<T> : NetworkValueCollection<T>
    {
        public BindableNetworkValueCollection(short port, string guid)
            : base(port, guid)
        {
            WPFSyncHelper = new ThreadSaveViewModelBase();
            LocalValues = new ThreadSaveObservableCollection<T>();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            WPFSyncHelper.ThreadSaveAction(() =>
            {
                base.OnCollectionChanged(e);                
            });
        }

        public ThreadSaveViewModelActor WPFSyncHelper { get; private set; }

//        public async override Task<bool> Connect(string host)
//        {
//            ConnectedToHost = host;
//            var sendRequstMessage = await _tcpNetworkSernder.SendRequstMessage<string[]>(
//                new RequstMessage() { InfoState = NetworkCollectionProtocol.CollectionGetUsers }, host);

//            var users = sendRequstMessage;

//            if (users == null)
//            {
//                return false;
//            }

//            this.CollectionRecievers = new List<string>(users);
//#pragma warning disable 4014
//            _tcpNetworkSernder.SendMultiMessageAsync(
//#pragma warning restore 4014
//new MessageBase() { InfoState = NetworkCollectionProtocol.CollectionRegisterUser }, users);
//            Registerd = true;

//            foreach (var item in await GetCollection(host, this.Port))
//            {
//                LocalValues.Add(item);
//            }
//            return true;
//        }

//        protected override void PullAddMessage(MessageBase obj)
//        {
//            if (obj.Message is NetworkCollectionMessage)
//            {
//                var mess = obj.Message as NetworkCollectionMessage;
//                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
//                {
//                    lock (SyncRoot)
//                    {
//                        WPFSyncHelper.ThreadSaveAction(() =>
//                        {
//                            LocalValues.Add((T)mess.Value);
//                        });
//                    }
//                }
//            }
//        }

//        protected override void PullClearMessage(MessageBase obj)
//        {
//            if (obj.Message is NetworkCollectionMessage)
//            {
//                var mess = obj.Message as NetworkCollectionMessage;
//                if (mess.Guid != null && Guid.Equals(mess.Guid))
//                {
//                    WPFSyncHelper.ThreadSaveAction(() =>
//                    {
//                        lock (SyncRoot)
//                        {
//                            LocalValues.Clear();
//                        }
//                    });
//                }
//            }
//        }

//        protected override void PullRemoveMessage(MessageBase obj)
//        {
//            if (obj.Message is NetworkCollectionMessage)
//            {
//                var mess = obj.Message as NetworkCollectionMessage;
//                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
//                {
//                    lock (SyncRoot)
//                    {
//                        WPFSyncHelper.ThreadSaveAction(() =>
//                        {
//                            LocalValues.Remove((T)mess.Value);
//                        });
//                    }
//                }
//            }
//        }

//        public override async void Reload()
//        {
//            var collection = await GetCollection(ConnectedToHost, 1337);
//            lock (SyncRoot)
//            {
//                LocalValues.Clear();
//                foreach (var item in collection)
//                {
//                    T item1 = item;
//                    WPFSyncHelper.ThreadSaveAction(() =>
//                    {
//                        LocalValues.Add(item1);
//                    });
//                }
//            }
//        }
    }
}
