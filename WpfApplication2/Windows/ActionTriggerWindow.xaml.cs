using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JPB.WPFNetworkIntigration;

namespace WpfApplication2.Windows
{
    /// <summary>
    /// Interaction logic for ActionTriggerWindow.xaml
    /// </summary>
    public partial class ActionTriggerWindow : Window
    {
        public ActionTriggerWindow(BindableNetworkValueBag<string> networkValueCollection)
        {
            InitializeComponent();
            this.DataContext = new ActionTriggerViewModel(networkValueCollection);
            Application.Current.MainWindow.Closing += (sender, args) => this.Close();
        }
    }
}
