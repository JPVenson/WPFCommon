using System.Windows;
using System.Windows.Markup;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Inverts the result of the Trigger
	/// </summary>
	[ContentProperty(nameof(TriggerStep))]
	public class InverseTrigger : DelegatorTriggerStepBase
	{
		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return !TriggerStep.Evaluate(dataContext, dependencyObject);
		}
	}
}