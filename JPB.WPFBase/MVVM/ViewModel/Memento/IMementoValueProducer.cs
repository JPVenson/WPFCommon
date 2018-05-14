using System.Collections.Concurrent;
using System.Collections.Generic;
using JPB.WPFBase.MVVM.ViewModel.Memento.Snapshots;

namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	public interface IMementoValueProducer
	{
		int CurrentAge { get; }
		bool Ignore { get; set; }
		object LockRoot { get; }
		IEnumerable<IMementoDataStamp> MementoDataStamps { get; }
		string PropertyName { get; }

		bool CanGoInHistory(int ages);
		void Forget();
		void GoInHistory(MementoViewModelBase viewModel, int ages);
	}

	internal interface IMementoValueHolder : IMementoValueProducer
	{
		ConcurrentStack<IMementoDataStamp> MementoData { get; set; }
		bool TryAdd(MementoViewModelBase originator, IMementoDataStamp stemp);
		object GetValue(object orignator);

		MementoPropertySnaptshot CreateSnapshot();
	}
}