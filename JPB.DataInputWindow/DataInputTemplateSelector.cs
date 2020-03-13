using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JPB.DataInputWindow.ViewModel;

namespace JPB.DataInputWindow
{
	public class DataInputTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var contentControl = container as ContentPresenter;
			if (contentControl != null)
			{
				if (!(item is DataImportFieldViewModelBase importFieldVm))
				{
					return null;
				}

				var findResource = contentControl.TryFindResource(typeof(DataInputDataTemplateService)) as DataInputDataTemplateService;
				if (findResource == null)
				{
					findResource = DataInputDataTemplateService.Default;
				}

				return findResource.DataTemplates.FirstOrDefault(e => e.DisplayKey == importFieldVm.DisplayKey)
					?.DataTemplate;
			}

			return null;
		}
	}
}
