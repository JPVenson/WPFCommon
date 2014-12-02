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
    public class BindableNetworkValueBag<T> : NetworkValueBag<T>
    {
        public BindableNetworkValueBag(short port, string guid)
            : base(port, guid)
        {
            WpfSyncHelper = new ThreadSaveViewModelBase();
            LocalValues = new ThreadSaveObservableCollection<T>();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            WpfSyncHelper.ThreadSaveAction(() =>
            {
                base.OnCollectionChanged(e);                
            });
        }

        public ThreadSaveViewModelActor WpfSyncHelper { get; private set; }
    }
}
