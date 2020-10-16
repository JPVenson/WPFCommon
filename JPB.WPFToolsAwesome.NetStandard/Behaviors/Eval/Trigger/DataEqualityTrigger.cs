using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     Checks if both bindings are equal
	/// </summary>
	//[XamlSetTypeConverter("ReceiveTypeConverter")]
	public class DataEqualityTrigger : TriggerStepBase
	{
		public static readonly DependencyProperty ValueProperty;

		static DataEqualityTrigger()
		{
			ValueProperty = Register(
				nameof(Value), typeof(object), typeof(EqualityTrigger), new PropertyMetadata(default(object)));
		}
		
		/// <summary>
		///		The property of the source object
		/// </summary>
		[Ambient]
		[Localizability(LocalizationCategory.None, Modifiability = Modifiability.Unmodifiable, Readability = Readability.Unreadable)]
		public DependencyProperty Property { get; set; }
		
		/// <summary>
		///		The value to be compared with
		/// </summary>
		[DependsOn("Property")]
		[TypeConverter(typeof (SetterTriggerConditionValueConverter))]
		public object Value
		{
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			return Equals(Value, dependencyObject.GetValue(Property));
		}
	}
}