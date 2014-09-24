using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JPB.DynamicInputBox;
using JPB.DynamicInputBox.InfoWindow;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IEnumerable<string> manifests = Directory.EnumerateFiles(@"\\s-cl-gen01\x.staff\jean.bachmann\BatchRemoteUpdate\", "*.xml");
            string question = manifests.Aggregate("Choosefile", (current, item) => current + ("#q" + item));
            string showInput = InputWindow.ReparseList(manifests, InputWindow.ShowInput(question, EingabeModus.RadioBox));
        }
    }
}
