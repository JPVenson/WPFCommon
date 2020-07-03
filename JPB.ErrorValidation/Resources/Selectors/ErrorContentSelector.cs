using System.Windows;
using System.Windows.Controls;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation.Resources.Selectors
{
	public class ErrorContentSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is IValidation)
			{
				return (container as FrameworkElement).FindResource("Validation") as DataTemplate;
			}

			return null;
		}
	}
}
