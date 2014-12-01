using System;
using System.ComponentModel;
using System.Linq;
using JPB.Communication.ComBase.Messages;

namespace JPB.Communication.Shared
{
    public class NetworkCollection<T> : NetworkValueCollection<T>
        where T : class,
            INotifyPropertyChanged,
            IUniqItem,
            new()
    {
        public NetworkCollection(short port, string guid)
            : base(port, guid)
        {
            base.TcpNetworkReceiver.RegisterChanged(pPullPropertyChanged, NetworkCollectionProtocol.CollectionUpdateItem);
        }

        private void pPullPropertyChanged(MessageBase obj)
        {
            PullPropertyChanged(obj);
        }

        protected void PullPropertyChanged(MessageBase obj)
        {
            if (obj.Message is NetworkCollectionMessage)
            {
                var mess = obj.Message as NetworkCollectionMessage;
                if (mess.Guid != null && Guid.Equals(mess.Guid) && mess.Value is T)
                {
                    var updateInfo = mess.Value as UpdateItemPropertyWrapper;

                    lock (SyncRoot)
                    {
                        var localItem = LocalValues.FirstOrDefault(s => s.Guid == updateInfo.Guid);
                        if (localItem == null)
                            return;
                        typeof(T).GetProperty(updateInfo.Property).SetValue(localItem, updateInfo.Value);
                    }
                }
            }
        }

        public override void Add(T item)
        {
            base.Add(item);
            item.PropertyChanged += item_PropertyChanged;
        }

        [Serializable]
        public class UpdateItemPropertyWrapper
        {
            public UpdateItemPropertyWrapper()
            {

            }

            public object Guid { get; set; }
            public string Property { get; set; }
            public object Value { get; set; }
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var obj = sender as T;
            if (obj == null)
                return;

            var changedProperty = typeof(T).GetProperty(e.PropertyName).GetValue(obj);

            base.SendPessage(NetworkCollectionProtocol.CollectionUpdateItem, new UpdateItemPropertyWrapper()
            {
                Property = e.PropertyName,
                Value = changedProperty,
                Guid = obj.Guid
            });
        }
    }
}