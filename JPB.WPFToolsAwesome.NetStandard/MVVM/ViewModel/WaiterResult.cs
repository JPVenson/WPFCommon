﻿using System.Threading;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel
{
	public class WaiterResult<T>
	{
		public WaiterResult(ManualResetEventSlim waiter)
		{
			Waiter = waiter;
		}
		public ManualResetEventSlim Waiter { get; private set; }
		public T Result { get; internal set; }
	}
}