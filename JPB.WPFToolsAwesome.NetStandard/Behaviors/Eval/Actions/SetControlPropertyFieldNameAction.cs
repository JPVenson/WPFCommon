using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JPB.WPFToolsAwesome.Behaviors.Eval.Trigger;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Actions
{
	/// <summary>
	///		Will set the property found in the AssociatedObject
	/// </summary>
	[ContentProperty(nameof(Converter))]
	public class SetControlPropertyFieldNameAction : TriggerActionBase
	{
		public static readonly DependencyProperty FieldNameProperty;
		public static readonly DependencyProperty ConverterProperty;

		static SetControlPropertyFieldNameAction()
		{
			FieldNameProperty = DependencyProperty.Register(
				nameof(FieldName), typeof(string), typeof(SetControlPropertyFieldNameAction), new PropertyMetadata(default(string)));

			ConverterProperty = DependencyProperty.Register(
				nameof(Converter), typeof(IValueConverter), typeof(SetControlPropertyFieldNameAction), new PropertyMetadata(default(IValueConverter)));
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
			var firstOrDefault = TriggerStepBase.GetDependencyProperties(control.GetType(), true)
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