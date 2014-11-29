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
            WPFSyncHelper = new ThreadSaveViewModelBase(Dispatcher.CurrentDispatcher);
        }

        public ThreadSaveViewModelActor WPFSyncHelper { get; private set; }

        protected override void PullAddMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
                {
                    lock (SyncRoot)
                    {
                        WPFSyncHelper.ThreadSaveAction(() =>
                        {
                            _localValues.Add((T)mess.Value);
                        });
                    }
                }
            }
        }

        protected override void PullClearMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid))
                {
                    WPFSyncHelper.ThreadSaveAction(() =>
                    {
                        lock (SyncRoot)
                        {
                            _localValues.Clear();
                        }
                    });
                }
            }
        }

        protected override void PullRemoveMessage(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
                {
                    lock (SyncRoot)
                    {
                        WPFSyncHelper.ThreadSaveAction(() =>
                        {
                            _localValues.Remove((T)mess.Value);
                        });
                    }
                }
            }
        }

        public override async void Reload()
        {
            var collection = await GetCollection(ConnectedToHost, 1337);
            lock (SyncRoot)
            {
                _localValues.Clear();
                foreach (var item in collection)
                {
                    T item1 = item;
                    WPFSyncHelper.ThreadSaveAction(() =>
                    {
                        _localValues.Add(item1);
                    });
                }
            }
        }
    }
}
