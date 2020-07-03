using System.Windows.Markup;

namespace JPB.WPFBase.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Inverts the result of the Evaluator
	/// </summary>
	[ContentProperty("EvaluatorStep")]
	public class InverseEvaluator : DelegatorEvaluatorStepBase
	{
		public override bool Evaluate(object dataContext)
		{
			return !EvaluatorStep.Evaluate(dataContext);
		}
	}
}