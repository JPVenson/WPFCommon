using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Base step for an Evaluator
	/// </summary>
	[ContentProperty(nameof(TriggerStep))]
	public abstract class DelegatorTriggerStepBase : TriggerStepBase
	{
		/// <summary>
		///		The trigger to be evaluated
		/// </summary>
		public ITriggerStep TriggerStep { get; set; }

		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return false;
		}

		public override void SetDataContext(object dataContext)
		{
			TriggerStep.PropertyChanged -= TriggerStepOnPropertyChanged;
			TriggerStep.PropertyChanged += TriggerStepOnPropertyChanged;
			TriggerStep.SetDataContext(dataContext);
			base.SetDataContext(dataContext);
		}

		private void TriggerStepOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(e.PropertyName);
		}
	}
}