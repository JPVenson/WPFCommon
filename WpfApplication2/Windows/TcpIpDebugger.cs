using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.WPFBase.MVVM.ViewModel;

namespace WpfApplication2.Windows
{
    class TcpIpDebugger : AsyncViewModelBase
    {
        public TcpIpDebugger()
        {
            Actions = new ThreadSaveObservableCollection<string>();
            Networkbase.OnNewItemLoadedSuccess += TCPNetworkReceiver_OnIncommingMessage;
            Networkbase.OnMessageSend += NetworkbaseOnOnMessageSend;
        }

        private void NetworkbaseOnOnMessageSend(MessageBase mess, ushort port1)
        {
            base.BeginThreadSaveAction(() =>
            {
                Actions.Add("OUT : " + mess.InfoState.ToString());
            });
        }

        private void TCPNetworkReceiver_OnIncommingMessage(MessageBase mess, ushort port1)
        {
            base.BeginThreadSaveAction(() =>
            {
                Actions.Add("IN  : " + mess.InfoState.ToString());
            });
        }

        private ThreadSaveObservableCollection<string> _actions;

        public ThreadSaveObservableCollection<string> Actions
        {
            get { return _actions; }
            set
            {
                _actions = value;
                SendPropertyChanged(() => Actions);
            }
        }
    }
}
