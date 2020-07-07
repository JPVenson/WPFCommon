using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Allows to run one or multiple validators
	/// </summary>
	[ContentProperty(nameof(TriggerSteps))]
	public abstract class MultiDelegatorTriggerStepBase : TriggerStepBase
	{
		public MultiDelegatorTriggerStepBase()
		{
			TriggerSteps = new StepsCollection();
		}

		public StepsCollection TriggerSteps { get; set; }

		public override void SetDataContext(object dataContext)
		{
			foreach (var step in TriggerSteps)
			{
				step.PropertyChanged -= TriggerStepOnPropertyChanged;
				step.PropertyChanged += TriggerStepOnPropertyChanged;
			}

			foreach (var triggerStep in TriggerSteps)
			{
				triggerStep.SetDataContext(dataContext);
			}

			base.SetDataContext(dataContext);
		}

		private void TriggerStepOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(e.PropertyName);
		}

		public class StepsCollection : List<ITriggerStep>
		{
		}
	}
}