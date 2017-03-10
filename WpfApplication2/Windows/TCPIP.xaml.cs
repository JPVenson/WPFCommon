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

namespace WpfApplication2.Windows
{
    /// <summary>
    /// Interaction logic for TCPIP.xaml
    /// </summary>
    public partial class TCPIP : Window
    {
        public TCPIP()
        {
            InitializeComponent();
            this.DataContext = new TcpIpDebugger();

            Application.Current.MainWindow.Closing += (sender, args) =>
            {
               this.Close();
            };
        }
    }
}
