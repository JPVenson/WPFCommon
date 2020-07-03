using System.Linq;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Evaluates all Evaluators that are nested to this one and returns true if all of them evaluate to true
	/// </summary>
	public class AndEvaluator : MultiDelegatorEvaluatorStepBase
	{
		public override bool Evaluate(object dataContext)
		{
			return EvaluatorSteps.All(f => f.Evaluate(dataContext));
		}
	}
}