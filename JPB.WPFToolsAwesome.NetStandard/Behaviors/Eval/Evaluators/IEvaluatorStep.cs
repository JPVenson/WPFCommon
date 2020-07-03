using System.ComponentModel;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Evaluators
{
	public interface IEvaluatorStep : INotifyPropertyChanged
	{
		bool Evaluate(object dataContext);
		void SetDataContext(object dataContext);
	}
}