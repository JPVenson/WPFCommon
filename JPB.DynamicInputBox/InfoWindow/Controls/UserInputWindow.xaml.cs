﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace JPB.DynamicInputBox.InfoWindow.Controls
{
    public partial class UserInputWindow : Window
    {
        internal UserInputWindow(List<object> inputQuestions, Func<ObservableCollection<object>> returnlist, IEnumerable<InputMode> eingabeModi, Dispatcher fromThread)
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
            {
	            vm.IsClosing = true;
            }
        }
    }
}