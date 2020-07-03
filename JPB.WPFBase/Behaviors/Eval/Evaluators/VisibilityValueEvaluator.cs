using System.Windows;

namespace JPB.WPFBase.Behaviors.Eval.Evaluators
{
	/// <summary>
	///		Binds the result of the Evaluator to a visibility and returns true if the visibility is <see cref="Visibility.Visible"/>
	/// </summary>
	public class VisibilityValueEvaluator : EvaluatorStepBase
	{
		static VisibilityValueEvaluator()
		{
			BindingProperty = Register(
			"Binding", typeof(System.Windows.Visibility), typeof(VisibilityValueEvaluator),
			new PropertyMetadata(default(System.Windows.Visibility)));
		}

		public static readonly DependencyProperty BindingProperty;
		
		public System.Windows.Visibility Binding
		{
			get { return (System.Windows.Visibility) GetValue(BindingProperty); }
			set { SetValue(BindingProperty, value); }
		}

		public override bool Evaluate(object dataContext)
		{
			return Binding == System.Windows.Visibility.Visible;
		}
	}
}