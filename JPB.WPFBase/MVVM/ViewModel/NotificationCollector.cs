using System;
using System.Collections.Generic;

namespace JPB.WPFBase.MVVM.ViewModel
{
	internal class NotificationCollector : IDisposable
	{
		private readonly ViewModelBase _vm;

		public NotificationCollector(ViewModelBase vm)
		{
			SendNotifications = new HashSet<string>();
			_vm = vm;
		}

		public HashSet<string> SendNotifications { get; private set; }

		public void Dispose()
		{
			_vm.DeferredNotification = null;
			foreach (var notification in SendNotifications)
			{
				_vm.SendPropertyChanged(notification);
			}
			SendNotifications.Clear();
		}
	}
}