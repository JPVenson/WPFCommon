using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace JPB.DynamicInputBox.InfoWindow.Controls
{
    public partial class UserInputWindow : Window
    {
        internal UserInputWindow(List<object> inputQuestions, Func<List<object>> returnlist, IEnumerable<EingabeModus> eingabeModi, Dispatcher fromThread)
        {
            InitializeComponent();
            DataContext = new UserInputViewModel(inputQuestions, returnlist, () =>
            {
                DialogResult = true;
                Close();
            }, eingabeModi, fromThread);
        }

        private void UserInputWindow_OnClosing(object sender, CancelEventArgs e)
        {
            var vm = (DataContext as UserInputViewModel);
            if (!vm.IsClosing)
                vm.IsClosing = true;
        }
    }
}