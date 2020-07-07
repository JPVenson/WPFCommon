using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Binds the result of the Evaluator to a property
	/// </summary>
	public class ValueTrigger : TriggerStepBase
	{
		public static readonly DependencyProperty BindingProperty;

		static ValueTrigger()
		{
			BindingProperty = Register(
				"Binding", typeof(bool), typeof(ValueTrigger), new PropertyMetadata(default(bool)));
		}

		public bool Binding
		{
			get { return (bool) GetValue(BindingProperty); }
			set { SetValue(BindingProperty, value); }
		}

		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return Binding;
		}
	}
}