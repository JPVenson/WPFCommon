using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JPB.WPFToolsAwesome.Behaviors.Eval.Evaluators;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Will set the property found in the AssociatedObject
	/// </summary>
	[ContentProperty("Converter")]
	public class SetControlPropertyFieldNameAction : EvaluateActionBase
	{
		public static readonly DependencyProperty FieldNameProperty;
		public static readonly DependencyProperty ConverterProperty;

		static SetControlPropertyFieldNameAction()
		{
			FieldNameProperty = DependencyProperty.Register(
				"FieldName", typeof(string), typeof(SetControlPropertyFieldNameAction), new PropertyMetadata(default(string)));

			ConverterProperty = DependencyProperty.Register(
				"Converter", typeof(IValueConverter), typeof(SetControlPropertyFieldNameAction), new PropertyMetadata(default(IValueConverter)));
		}

		public IValueConverter Converter
		{
			get { return (IValueConverter)GetValue(ConverterProperty); }
			set { SetValue(ConverterProperty, value); }
		}

		public string FieldName
		{
			get { return (string)GetValue(FieldNameProperty); }
			set { SetValue(FieldNameProperty, value); }
		}

		public override void SetValue(DependencyObject control, object dataContext, bool state)
		{
			var firstOrDefault = EvaluatorStepBase.GetDependencyProperties(control.GetType(), true)
				.FirstOrDefault(e => e.Name.Equals(FieldName));
			object value = state;
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