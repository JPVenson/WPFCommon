using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JPB.Communication;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.DynamicInputBox;
using JPB.DynamicInputBox.InfoWindow;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using JPB.WPFNetworkIntigration;
using WpfApplication2.Windows;

namespace WpfApplication2
{
    public class MainWindowViewModel : AsyncViewModelBase
    {
        public MainWindowViewModel()
        {
            AddToListCommand = new DelegateCommand(ExecuteAddToList, CanExecuteAddToList);
            RemoveSelectedFromListCommand = new DelegateCommand(ExecuteRemoveSelectedFromList, CanExecuteRemoveSelectedFromList);
            NetworkValueCollection = new BindableNetworkValueCollection<string>(1337, "test");

            string showInput;
            IPAddress buffOutput;
            do
            {
                showInput = InputWindow.ShowInput("Connect to: ", EingabeModus.Text) as string;
                if(string.IsNullOrEmpty(showInput))
                    break;
            } while (!IPAddress.TryParse(showInput, out buffOutput));

            var master = "192.168.1.10";
            var slave = "192.168.1.5";
            if (!string.IsNullOrEmpty(showInput))
            {
                NetworkValueCollection.Connect(showInput);
            }

            new TCPIP().Show();
            new ActionTriggerWindow(NetworkValueCollection).Show();

            var rec = new Window();

            Application.Current.MainWindow.Closing += (sender, args) => rec.Close();

            rec.SizeToContent = SizeToContent.WidthAndHeight;
            rec.Title = "Reciever";
            var button = new Button();
            var recs = new ListBox();

            button.Content = "Refresh";
            button.Click += (sender, args) =>
            {
                recs.Items.Clear();
                foreach (var item in NetworkValueCollection.CollectionRecievers)
                {
                    recs.Items.Add(item);
                }
            };
            var dp = new DockPanel();
            dp.Children.Add(button);
            dp.Children.Add(recs);
            rec.Content = dp;
            rec.Show();


            
        }

        private string _inputText;

        public string InputText
        {
            get { return _inputText; }
            set
            {
                _inputText = value;
                SendPropertyChanged(() => InputText);
            }
        }

        private string _selectedText;

        public string SelectedText
        {
            get { return _selectedText; }
            set
            {
                _selectedText = value;
                SendPropertyChanged(() => SelectedText);
            }
        }

        private BindableNetworkValueCollection<string> _networkValueCollection;

        public BindableNetworkValueCollection<string> NetworkValueCollection
        {
            get { return _networkValueCollection; }
            set
            {
                _networkValueCollection = value;
                SendPropertyChanged(() => NetworkValueCollection);
            }
        }

        public DelegateCommand AddToListCommand { get; private set; }

        public void ExecuteAddToList(object sender)
        {
            NetworkValueCollection.Add(InputText);
        }

        public bool CanExecuteAddToList(object sender)
        {
            return true;
        }

        public DelegateCommand RemoveSelectedFromListCommand { get; private set; }

        public void ExecuteRemoveSelectedFromList(object sender)
        {
            NetworkValueCollection.Remove(SelectedText);
        }

        public bool CanExecuteRemoveSelectedFromList(object sender)
        {
            return !string.IsNullOrEmpty(SelectedText);
        }
    }
}
