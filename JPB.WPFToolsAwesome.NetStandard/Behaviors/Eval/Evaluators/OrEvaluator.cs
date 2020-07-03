using System.Linq;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Checks if Any Evaluator returns true
	/// </summary>
	public class OrEvaluator : MultiDelegatorEvaluatorStepBase
	{
		public override bool Evaluate(object dataContext)
		{
			return EvaluatorSteps.Any(f => f.Evaluate(dataContext));
		}
	}
}