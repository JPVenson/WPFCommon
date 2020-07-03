using System.Collections.Generic;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Memento
{
	public interface IMementoValueProducer
	{
		int CurrentAge { get; }
		bool Ignore { get; set; }
		IEnumerable<IMementoDataStamp> MementoDataStamps { get; }
		string PropertyName { get; }

		bool CanGoInHistory(int ages);
		void Forget();
		void GoInHistory(MementoViewModelBase viewModel, int ages);
	}
}