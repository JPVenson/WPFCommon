using JPB.ErrorValidation;
using JPB.ErrorValidation.ViewModelProvider.Base;

namespace JPB.DataInputWindow.ViewModel
{
	public abstract class DataImportFieldViewModelBase : AsyncErrorProviderBase
	{
		public DataImportFieldViewModelBase(IErrorCollectionBase errors) : base(errors)
		{
			
		}

		private object _value;
		private DisplayTypes _displayKey;
		private object _caption;

		public string Key { get; set; }

		public object Caption
		{
			get { return _caption; }
			set
			{
				SendPropertyChanging(() => Caption);
				_caption = value;
				SendPropertyChanged(() => Caption);
			}
		}

		public DisplayTypes DisplayKey
		{
			get { return _displayKey; }
			set
			{
				SendPropertyChanging(() => DisplayKey);
				_displayKey = value;
				SendPropertyChanged(() => DisplayKey);
			}
		}

		public object Value
		{
			get { return _value; }
			set
			{
				SendPropertyChanging(() => Value);
				_value = value;
				SendPropertyChanged(() => Value);
			}
		}
	}

	[DataInputTypeAttribute(DisplayTypes.Text)]
	[DataInputTypeAttribute(DisplayTypes.Date)]
	public class SimpleDataImportFieldViewModelBase : DataImportFieldViewModelBase
	{
		public SimpleDataImportFieldViewModelBase(IErrorCollectionBase errors) : base(errors)
		{
		}
	}
}