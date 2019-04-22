using System.Collections.Concurrent;
using JPB.WPFBase.MVVM.ViewModel.Memento.Snapshots;

namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	internal interface IMementoValueHolder : IMementoValueProducer
	{
		ConcurrentStack<IMementoDataStamp> MementoData { get; set; }
		bool TryAdd(MementoViewModelBase originator, IMementoDataStamp stemp);
		object GetValue(MementoViewModelBase orignator);

		MementoPropertySnaptshot CreateSnapshot();
	}
}