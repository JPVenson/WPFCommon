using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Binds the result of the Trigger to a visibility and returns true if the visibility is
	///     <see cref="Visibility.Visible" />
	/// </summary>
	public class VisibilityValueTrigger : TriggerStepBase
	{
		public static readonly DependencyProperty BindingProperty;

		static VisibilityValueTrigger()
		{
			BindingProperty = Register(
				"Binding", typeof(Visibility), typeof(VisibilityValueTrigger),
				new PropertyMetadata(default(Visibility)));
		}

		public Visibility Binding
		{
			get { return (Visibility) GetValue(BindingProperty); }
			set { SetValue(BindingProperty, value); }
		}

		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return Binding == Visibility.Visible;
		}
	}
}