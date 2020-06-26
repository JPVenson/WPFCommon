using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NUnit.Framework;

[SetUpFixture]
public class WpfBootstrapper
{
	private Thread _appThread;

	public static Application App { get; private set; }

	public static async Task WaitForDispatcher()
	{
		await App.Dispatcher.BeginInvoke(new Action(() => Console.Write("")), DispatcherPriority.ApplicationIdle);
	}

	[OneTimeSetUp]
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
		_appThread.Name = "Dispatcher Thread";
		_appThread.Start();

		appCreated.Wait();
		//DispatcherLock.Current = new DispatcherLock(App.Dispatcher);
	}

	[OneTimeTearDown]
	public void TearDown()
	{
		App.Dispatcher.Invoke(() => { App.Shutdown(); });
		if (_appThread.IsAlive)
		{
			_appThread.Interrupt();
			_appThread.Abort();
		}
	}
}