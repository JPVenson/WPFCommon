using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///		Can be used to Capture the Application Dispatcher across threads
	/// </summary>
	internal class DispatcherLock : IDisposable
	{
		static DispatcherLock()
		{
			Current = new AsyncLocal<DispatcherLock>();
		}

		private readonly Dispatcher _currentDispatcher;

		internal static AsyncLocal<DispatcherLock> Current;

		/// <summary>
		///		Gets the Captured Dispatcher or the current Application dispatcher.
		///		If no Dispatcher was Captured from another thread and its called from another thread, it will return null
		/// </summary>
		/// <returns></returns>
		public static Dispatcher GetDispatcher()
		{
			return Current.Value?._currentDispatcher ?? Application.Current.Dispatcher;
		}

		internal DispatcherLock(Dispatcher currentDispatcher)
		{
			_currentDispatcher = currentDispatcher;
		}

		/// <summary>
		///		Captures the Dispatcher from the current Application inside the thread
		/// </summary>
		/// <returns>An IDisposable object that can be used to remove the assosiation</returns>
		public static DispatcherLock CaptureDispatcher()
		{
			return Current.Value ?? (Current.Value = new DispatcherLock(Application.Current.Dispatcher));
		}

		/// <summary>
		///		Removes the Assosiation from the Current thread
		/// </summary>
		public void Dispose()
		{
			Current.Value = null;
		}
	}
}