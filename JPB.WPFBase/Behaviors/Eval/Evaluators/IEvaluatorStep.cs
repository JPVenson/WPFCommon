using System.ComponentModel;

namespace JPB.WPFBase.Behaviors.Eval.Evaluators
{
	public interface IEvaluatorStep : INotifyPropertyChanged
	{
		bool Evaluate(object dataContext);
		void SetDataContext(object dataContext);
	}
}