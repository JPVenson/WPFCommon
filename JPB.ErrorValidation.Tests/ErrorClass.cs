using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using JPB.WPFToolsAwesome.Error.ValidationRules;
using JPB.WPFToolsAwesome.Error.ViewModelProvider.Base;

namespace JPB.ErrorValidation.Tests
{
	public class ErrorClass : AsyncErrorProviderBase<NoErrors>
	{
		public class ErrorClassErrors : ErrorCollection<ErrorClass>
		{
			public ErrorClassErrors()
			{
				Add(new Error<ErrorClass>("NumberProperty.IsZero", e => e.NumberProperty == 0,
					nameof(NumberProperty)));
				Add(new Error<ErrorClass>("NumberProperty.OneHundered", e => e.NumberProperty == 100,
					nameof(NumberProperty)));
				Add(new Error<ErrorClass>("NumberProperty.Zero&ErrorText.Empty", 
					e => e.NumberProperty == 0 && string.IsNullOrWhiteSpace(e.TextProperty),
					nameof(NumberProperty)));
			}
		}

		private string _textProperty;
		private int _numberProperty;

		public int NumberProperty
		{
			get { return _numberProperty; }
			set
			{
				SendPropertyChanging(() => NumberProperty);
				_numberProperty = value;
				SendPropertyChanged(() => NumberProperty);
			}
		}

		public string TextProperty
		{
			get { return _textProperty; }
			set
			{
				SendPropertyChanging(() => TextProperty);
				_textProperty = value;
				SendPropertyChanged(() => TextProperty);
			}
		}
	}
}
