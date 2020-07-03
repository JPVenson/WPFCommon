using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;

namespace JPB.WPFBase.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Allows to run one or multiple validators
	/// </summary>
	[ContentProperty("EvaluatorSteps")]
	public abstract class MultiDelegatorEvaluatorStepBase : EvaluatorStepBase
	{
		public MultiDelegatorEvaluatorStepBase()
		{
			EvaluatorSteps = new StepsCollection();
		}

		public StepsCollection EvaluatorSteps { get; set; }
		
		public override void SetDataContext(object dataContext)
		{
			foreach (var step in EvaluatorSteps)
			{
				step.PropertyChanged -= EvaluatorStepOnPropertyChanged;
				step.PropertyChanged += EvaluatorStepOnPropertyChanged;
			}
			foreach (var evaluatorStep in EvaluatorSteps)
			{
				evaluatorStep.SetDataContext(dataContext);
			}
			base.SetDataContext(dataContext);
		}

		private void EvaluatorStepOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(e.PropertyName);
		}

		public class StepsCollection : List<IEvaluatorStep>
		{
		}
	}
}