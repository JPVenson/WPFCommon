using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JPB.WPFToolsAwesome.Behaviors.Eval.Trigger;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Updates the value of a Binding to ether the <see cref="FalseValue"/> or <see cref="TrueValue"/> bindings
	/// </summary>
	public class SetFieldAction : TriggerActionBase
	{
		public static readonly DependencyProperty FieldNameProperty;
		public static readonly DependencyProperty TrueValueProperty;
		public static readonly DependencyProperty FalseValueProperty;
		public static readonly DependencyProperty ConverterProperty;

		static SetFieldAction()
		{
			FieldNameProperty = DependencyProperty.Register(
				nameof(FieldName), typeof(object), typeof(SetFieldAction), new FrameworkPropertyMetadata(default(object)));
			TrueValueProperty = DependencyProperty.Register(
				nameof(TrueValue), typeof(object), typeof(SetFieldAction), new FrameworkPropertyMetadata(default(object)));
			FalseValueProperty = DependencyProperty.Register(
				nameof(FalseValue), typeof(object), typeof(SetFieldAction), new FrameworkPropertyMetadata(default(object)));
			ConverterProperty = DependencyProperty.Register(
				nameof(Converter), typeof(IValueConverter), typeof(SetControlPropertyFieldNameAction), new PropertyMetadata(default(IValueConverter)));

		}

		public SetFieldAction()
		{
			
		}


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

		public string FieldName
		{
			get { return (string) GetValue(FieldNameProperty); }
			set { SetValue(FieldNameProperty, value); }
		}

		public IValueConverter Converter
		{
			get { return (IValueConverter)GetValue(ConverterProperty); }
			set { SetValue(ConverterProperty, value); }
		}

		public override void SetValue(DependencyObject control, object dataContext, bool state)
		{
			var firstOrDefault = TriggerStepBase.GetDependencyProperties(control.GetType(), true)
				.FirstOrDefault(e => e.Name.Equals(FieldName));
			object value = state ? TrueValue : FalseValue;
			if (firstOrDefault != null)
			{
				if (Converter != null)
				{
					value = Converter.Convert(value, firstOrDefault.PropertyType, null,
						CultureInfo.CurrentUICulture);
				}

				control.SetValue(firstOrDefault, value);
			}
		}
	}
}