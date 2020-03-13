using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace JPB.DataInputWindow
{
	[ContentProperty(nameof(DataTemplates))]
	public class DataInputDataTemplateService : DependencyObject
	{
		static DataInputDataTemplateService()
		{
			var resDict = new ResourceDictionary();
			resDict.BeginInit();
			resDict.Source = new Uri($"pack://application:,,,/{typeof(DataInputDataTemplateService).Assembly.GetName().Name};component/DefaultDataInputDataTemplateServiceResources.xaml");
			resDict.EndInit();
			Default = resDict["DefaultDataInputDataTemplateService"] as DataInputDataTemplateService;
		}

		public static DataInputDataTemplateService Default { get; private set; }

		public static readonly DependencyProperty DataTemplatesProperty = DependencyProperty.Register(
			"DataTemplates", typeof(DataTemplateCollection), typeof(DataInputDataTemplateService), new PropertyMetadata(new DataTemplateCollection()));

		public DataTemplateCollection DataTemplates
		{
			get { return (DataTemplateCollection) GetValue(DataTemplatesProperty); }
			set { SetValue(DataTemplatesProperty, value); }
		}
	}
}
