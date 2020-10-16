using System.Windows;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Updates the value of a Binding to ether the <see cref="FalseValue"/> or <see cref="TrueValue"/> bindings
	/// </summary>
	public class SetBindingAction : TriggerActionBase
	{
		public static readonly DependencyProperty BindingProperty;
		public static readonly DependencyProperty TrueValueProperty;

		static SetBindingAction()
		{
			BindingProperty = DependencyProperty.Register(
				nameof(Binding), typeof(object), typeof(SetBindingAction), new FrameworkPropertyMetadata(default(object)));
			TrueValueProperty = DependencyProperty.Register(
				nameof(TrueValue), typeof(object), typeof(SetBindingAction), new FrameworkPropertyMetadata(default(object)));
			FalseValueProperty = DependencyProperty.Register(
				nameof(FalseValue), typeof(object), typeof(SetBindingAction), new FrameworkPropertyMetadata(default(object)));
		}

		public SetBindingAction()
		{
			
		}

		public static readonly DependencyProperty FalseValueProperty;

		public object FalseValue
		{
			get { return (object) GetValue(FalseValueProperty); }
			set { SetValue(FalseValueProperty, value); }
		}

		public object TrueValue
		{
			get { return (object) GetValue(TrueValueProperty); }
			set { SetValue(TrueValueProperty, value); }
		}

		public object Binding
		{
			get { return (object) GetValue(BindingProperty); }
			set { SetValue(BindingProperty, value); }
		}

		public override void SetValue(DependencyObject control, object dataContext, bool state)
		{
			Binding = state ? TrueValue : FalseValue;
		}
	}
}