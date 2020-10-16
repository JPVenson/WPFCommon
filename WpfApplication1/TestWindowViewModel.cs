using System;
using System.Threading;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ValidationTyps;
using JPB.WPFToolsAwesome.Error.ViewModelProvider.Base;
using JPB.WPFToolsAwesome.MVVM.ViewModel;

namespace WpfApplication1
{
	public class TestWindowViewModel : AsyncErrorProviderBase<TestWindowViewModelRules>
	{
		private ThreadSaveObservableCollection<string> _threadSaveObservableCollection;
		private string _toValidationString;

		public TestWindowViewModel()
		{
			ThreadSaveObservableCollection = new ThreadSaveObservableCollection<string>();

			//SimpleWork(() =>
			//{
			//	while (true)
			//	{
			//		try
			//		{
			//			Thread.Sleep(250);
			//			foreach (var item in ThreadSaveObservableCollection)
			//			{
			//				Console.WriteLine("LOG " + item);
			//			}
			//		}
			//		catch (Exception e)
			//		{
			//			Console.WriteLine(e);
			//			throw;
			//		}
			//	}
			//});

			//SimpleWork(() =>
			//{
			//	var x = 0;
			//	while (true)
			//	{
			//		Thread.Sleep(1);
			//		var count = ThreadSaveObservableCollection.Count();
			//		ThreadSaveObservableCollection.Add("Test Nr " + x++);
			//	}
			//});

			//SimpleWork(() =>
			//{
			//	while (true)
			//	{
			//		Thread.Sleep(2);
			//		var count = ThreadSaveObservableCollection.Count();
			//		if (count > 0)
			//		{
			//			var elementAt = ThreadSaveObservableCollection.FirstOrDefault();
			//			ThreadSaveObservableCollection.Remove(elementAt);
			//		}
			//	}
			//});
		}

		public ThreadSaveObservableCollection<string> ThreadSaveObservableCollection
		{
			get { return _threadSaveObservableCollection; }
			set
			{
				SendPropertyChanging(() => ThreadSaveObservableCollection);
				_threadSaveObservableCollection = value;
				SendPropertyChanged(() => ThreadSaveObservableCollection);
			}
		}

		public string ToValidationString
		{
			get { return _toValidationString; }
			set
			{
				_toValidationString = value;
				SendPropertyChanged(() => ToValidationString);
			}
		}

		public void ExecuteTaskB(object sender)
		{
			Task.Run(new Func<Task>(async () =>
			{
				await base.BeginViewModelAction(() => { });
			}));
			Thread.CurrentThread.Join(10000);
			ToValidationString = 1337.ToString();
		}

		public bool CanExecuteTaskB(object sender)
		{
			Thread.CurrentThread.Join(2000);
			return true;
		}

		public void ExecuteTaskA(object sender)
		{
			Thread.CurrentThread.Join(10000);
		}

		public bool CanExecuteTaskA(object sender)
		{
			return true;
		}
	}

	public class TestWindowViewModelRules : ErrorCollection<TestWindowViewModel>
	{
		public TestWindowViewModelRules()
		{
			var run = 0;
			var vc_attributes = 0;
			Add(new Error<TestWindowViewModel>("Is null or empty", "ToValidationString",
					s => string.IsNullOrEmpty(s.ToValidationString))
				.And(new Error<TestWindowViewModel>("Is too big", "ToValidationString",
					s => s.ToValidationString != null && s.ToValidationString.Length > 5)));
			Add(new Error<TestWindowViewModel>("Must be Int", "ToValidationString",
				s => !int.TryParse(s.ToValidationString, out vc_attributes)));
			Add(new AsyncError<TestWindowViewModel>("Wait", "ToValidationString", async s =>
			{
				await Task.Delay(1000);
				return false;
			})
			{
				AsyncState = AsyncState.Async,
				RunState = AsyncRunState.CurrentPlusOne
			});
		}
	}
}