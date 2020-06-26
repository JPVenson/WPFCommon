using System;
using System.Windows;
using JPB.DataInputWindow.ViewModel;
using JPB.ErrorValidation;

namespace JPB.DataInputWindow
{
	public class DataInputBuilderFassade
	{
		internal readonly DataImportViewModel _dataImportViewModel;

		public DataInputBuilderFassade(DataImportViewModel dataImportViewModel)
		{
			_dataImportViewModel = dataImportViewModel;
		}

		public DataFieldBuilderFassade Field(DisplayTypes typeKey, string key)
		{
			return new DataFieldBuilderFassade(this, typeKey, key);
		}

		private ResourceDictionary ConstructResourceDictionary(Uri source)
		{
			var resDict = new ResourceDictionary();
			resDict.BeginInit();
			resDict.Source = source;
			resDict.EndInit();
			return resDict;
		}

		public DataImportViewModel Show(Func<Window> windowFactory = null)
		{
			windowFactory = windowFactory ?? (() => new Window()
			{
				SizeToContent = SizeToContent.WidthAndHeight,
				WindowStyle = WindowStyle.ToolWindow
			});
			var window = windowFactory();
			window.Content = _dataImportViewModel;
			foreach (var dataImportFieldViewModelBase in _dataImportViewModel.Fields)
			{
				dataImportFieldViewModelBase.LoadErrorMapperData();
				dataImportFieldViewModelBase.ForceRefreshAsync().GetAwaiter().GetResult();
			}
			var resDict = new ResourceDictionary();
			resDict.BeginInit();
			resDict.MergedDictionaries.Add(ConstructResourceDictionary(new Uri($"pack://application:,,,/{typeof(DataInputDataTemplateService).Assembly.GetName().Name};component/DataImportWindowResources.xaml")));
			resDict.EndInit();

			window.Resources.MergedDictionaries.Add(resDict);
			window.ShowDialog();
			return _dataImportViewModel;
		}
	}
}