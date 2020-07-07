using System.Linq;
using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Checks if Any Evaluator returns true
	/// </summary>
	public class OrTrigger : MultiDelegatorTriggerStepBase
	{
		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return TriggerSteps.Any(f => f.Evaluate(dataContext, dependencyObject));
		}
	}
}