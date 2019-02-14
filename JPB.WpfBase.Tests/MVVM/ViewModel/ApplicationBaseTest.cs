using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using JPB.WPFBase.MVVM.ViewModel;
using NUnit.Framework;

namespace JPB.WpfBase.Tests.MVVM.ViewModel
{
	public class ApplicationBaseTest
	{
		public static Application App { get; private set; }
		private Thread _appThread;

		[SetUp]
		public void Setup()
		{
			if (App != null)
			{
				return;
			}

			var appCreated = new ManualResetEventSlim();

			_appThread = new Thread(() =>
			{
				App = new Application();
				appCreated.Set();
				App.Run();
			});
			_appThread.SetApartmentState(ApartmentState.STA);
			_appThread.IsBackground = true;
			_appThread.Start();

			appCreated.Wait();
			//DispatcherLock.Current = new DispatcherLock(App.Dispatcher);
		}

		[TearDown]
		public void TearDown()
		{
			//App.Dispatcher.Invoke(() =>
			//{
			//	App.Shutdown();
			//});
			//if (_appThread.IsAlive)
			//{
			//	_appThread.Interrupt();
			//	_appThread.Abort();
			//}
		}
	}
}
