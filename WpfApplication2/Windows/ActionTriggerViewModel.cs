using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using JPB.WPFNetworkIntigration;

namespace WpfApplication2.Windows
{
    class ActionTriggerViewModel : AsyncViewModelBase
    {
        private readonly BindableNetworkValueBag<string> _networkValueCollection;

        public ActionTriggerViewModel(BindableNetworkValueBag<string> networkValueCollection)
        {
            _networkValueCollection = networkValueCollection;

            PullItemsAgainCommand = new DelegateCommand(ExecutePullItemsAgain, CanExecutePullItemsAgain);
            ClearCommand = new DelegateCommand(ExecuteClear, CanExecuteClear);
        }

        public DelegateCommand PullItemsAgainCommand { get; private set; }

        public async void ExecutePullItemsAgain(object sender)
        {
            _networkValueCollection.Reload();
        }

        public bool CanExecutePullItemsAgain(object sender)
        {
            return true;
        }

        public DelegateCommand ClearCommand { get; private set; }

        public void ExecuteClear(object sender)
        {
            _networkValueCollection.Clear();
        }

        public bool CanExecuteClear(object sender)
        {
            return true;
        }
    }
}
