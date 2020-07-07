using System.Linq;
using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Evaluates all Evaluators that are nested to this one and returns true if all of them evaluate to true
	/// </summary>
	public class AndTrigger : MultiDelegatorTriggerStepBase
	{
		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return TriggerSteps.All(f => f.Evaluate(dataContext, dependencyObject));
		}
	}
}