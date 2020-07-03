using System.Windows;
using System.Windows.Markup;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Will set the <see cref="Binding"/> with ether true or false
	/// </summary>
	[ContentProperty("Binding")]
	public class SetControlPropertyBindingAction : EvaluateActionBase
	{
		public static readonly DependencyProperty BindingProperty;
		static SetControlPropertyBindingAction()
		{
			BindingProperty = DependencyProperty.Register(
				"Binding", typeof(object), typeof(SetControlPropertyBindingAction), new PropertyMetadata(default(object)));
		}

		public bool Binding
		{
			get { return (bool)GetValue(BindingProperty); }
			set { SetValue(BindingProperty, value); }
		}

		public override void SetValue(DependencyObject control, object dataContext, bool state)
		{
			Binding = state;
		}
	}
}