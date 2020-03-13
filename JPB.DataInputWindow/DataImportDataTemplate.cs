using System.Windows;
using System.Windows.Markup;

namespace JPB.DataInputWindow
{
	[ContentProperty(nameof(DataTemplate))]
	public class DataImportDataTemplate : DependencyObject
	{
		public static readonly DependencyProperty DisplayKeyProperty = DependencyProperty.Register(
			"DisplayKey", typeof(DisplayTypes), typeof(DataImportDataTemplate), new PropertyMetadata(default(DisplayTypes)));

		public static readonly DependencyProperty DataTemplateProperty = DependencyProperty.Register(
			"DataTemplate", typeof(DataTemplate), typeof(DataImportDataTemplate), new PropertyMetadata(default(DataTemplate)));

		public DataTemplate DataTemplate
		{
			get { return (DataTemplate) GetValue(DataTemplateProperty); }
			set { SetValue(DataTemplateProperty, value); }
		}

		public DisplayTypes DisplayKey
		{
			get { return (DisplayTypes) GetValue(DisplayKeyProperty); }
			set { SetValue(DisplayKeyProperty, value); }
		}
	}
}