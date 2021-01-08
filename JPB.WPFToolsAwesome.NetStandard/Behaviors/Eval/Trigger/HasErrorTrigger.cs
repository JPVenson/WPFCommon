using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using JPB.WPFToolsAwesome.Error.ViewModelProvider.Base;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	public class CsvToStringCollectionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var stringCollection = new StringCollection();
			stringCollection.AddRange((value as string).Split(','));
			return stringCollection;
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			var ofType = (value as StringCollection).OfType<string>();
			return string.Join(",", ofType);
		}
	}

	/// <summary>
	///     Evaluates against the <see cref="AsyncErrorProviderBase" /> and checks if the view model has any errors
	/// </summary>
	[ContentProperty(nameof(Properties))]
	public class HasErrorTrigger : TriggerStepBase
	{
		public static readonly DependencyProperty PropertiesProperty;
		static HasErrorTrigger()
		{
			PropertiesProperty = Register(
				nameof(Properties), typeof(StringCollection), typeof(HasErrorTrigger), new PropertyMetadata(new StringCollection()));
		}

		/// <summary>
		///		Gets or Sets the properties that have to be checked for error validation. If left empty all properties will be checked
		/// </summary>
		[TypeConverter(typeof(CsvToStringCollectionConverter))]
		public StringCollection Properties
		{
			get { return (StringCollection) GetValue(PropertiesProperty); }
			set { SetValue(PropertiesProperty, value); }
		}

		public override void SetDataContext(object dataContext)
		{
			if (dataContext is INotifyDataErrorInfo errorProvider)
			{
				WeakEventManager<INotifyDataErrorInfo, DataErrorsChangedEventArgs>
					.RemoveHandler(errorProvider, nameof(INotifyDataErrorInfo.ErrorsChanged),
						ErrorProvider_ErrorsChanged);
				WeakEventManager<INotifyDataErrorInfo, DataErrorsChangedEventArgs>
					.AddHandler(errorProvider, nameof(INotifyDataErrorInfo.ErrorsChanged), ErrorProvider_ErrorsChanged);
			}

			base.SetDataContext(dataContext);
		}

		public override bool Evaluate(object dataContext, DependencyObject dependencyObject)
		{
			if (dataContext is AsyncErrorProviderBase errorProvider)
			{
				if (Properties.Count == 0)
				{
					return errorProvider.HasErrors;
				}

				return errorProvider.ActiveValidationCases.Any(f => f.ErrorIndicator.Any(e => Properties.Contains(e)));
			}
			else if (dataContext is INotifyDataErrorInfo errorInfo)
			{
				foreach (var property in Properties)
				{
					if (errorInfo.GetErrors(property).Cast<object>().Any())
					{
						return true;
					}
				}
				return false;
			}

			return false;
		}

		private void ErrorProvider_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.PropertyName) || Properties.Contains(e.PropertyName) || Properties.Count == 0)
			{
				OnPropertyChanged();
			}
		}
	}
}