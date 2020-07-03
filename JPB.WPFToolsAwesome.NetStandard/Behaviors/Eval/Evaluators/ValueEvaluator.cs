using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Binds the result of the Evaluator to a property
	/// </summary>
	public class ValueEvaluator : EvaluatorStepBase
	{
		static ValueEvaluator()
		{
			BindingProperty = Register(
				"Binding", typeof(bool), typeof(ValueEvaluator), new PropertyMetadata(default(bool)));
		}

		public static readonly DependencyProperty BindingProperty;

		public bool Binding
		{
			get { return (bool) GetValue(BindingProperty); }
			set { SetValue(BindingProperty, value); }
		}

		public override bool Evaluate(object dataContext)
		{
			return Binding;
		}
	}
}