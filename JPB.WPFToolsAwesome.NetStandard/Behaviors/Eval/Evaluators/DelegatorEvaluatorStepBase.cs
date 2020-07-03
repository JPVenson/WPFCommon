using System.ComponentModel;
using System.Windows.Markup;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Base step for an Evaluator
	/// </summary>
	[ContentProperty("EvaluatorStep")]
	public abstract class DelegatorEvaluatorStepBase : EvaluatorStepBase
	{
		public IEvaluatorStep EvaluatorStep { get; set; }

		public override bool Evaluate(object dataContext)
		{
			return false;
		}

		public override void SetDataContext(object dataContext)
		{
			EvaluatorStep.PropertyChanged -= EvaluatorStepOnPropertyChanged;
			EvaluatorStep.PropertyChanged += EvaluatorStepOnPropertyChanged;
			EvaluatorStep.SetDataContext(dataContext);
			base.SetDataContext(dataContext);
		}

		private void EvaluatorStepOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(e.PropertyName);
		}
	}
}