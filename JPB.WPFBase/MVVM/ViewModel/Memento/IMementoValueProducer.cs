using System.Collections.Generic;

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
}