using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using JPB.ErrorValidation.ValidationRules;
using JPB.ErrorValidation.ValidationTyps;
using JPB.ErrorValidation.ViewModelProvider.Base;
using NUnit.Framework;

namespace JPB.ErrorValidation.Tests
{
	public class ErrorClass : AsyncErrorProviderBase<ErrorClass.ErrorClassErrors>
	{
		public class ErrorClassErrors : ErrorCollection<ErrorClass>
		{
			public ErrorClassErrors()
			{
				Add(new Error<ErrorClass>("ErrorText.Empty", e => string.IsNullOrWhiteSpace(e.TextProperty),
					nameof(TextProperty)));
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

	public class WPFValidatorMock
	{
		private readonly INotifyDataErrorInfo _viewModel;

		public IDictionary<string, IEnumerable> ErrorsFromBindings { get; set; }

		public WPFValidatorMock(INotifyDataErrorInfo viewModel)
		{
			ErrorsFromBindings = new Dictionary<string, IEnumerable>();
			_viewModel = viewModel;
			_viewModel.ErrorsChanged += _viewModel_ErrorsChanged;
		}

		private void _viewModel_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
		{
			
		}

		public void Bind(string name)
		{
			var enumerable = _viewModel.GetErrors(name);
			ErrorsFromBindings.Add(name, enumerable);
		}
	}

	[TestFixture]
    public class AsyncErrorProviderTests
    {
	    public AsyncErrorProviderTests()
	    {
		    
	    }

	    public void TestValidation()
	    {
		    var validator = new ErrorClass();
		    var wpfValidator = new WPFValidatorMock(validator);
			validator.NumberProperty = 0;
			validator.TextProperty = "";

			var numberErrors = wpfValidator.ErrorsFromBindings[nameof(ErrorClass.NumberProperty)].OfType<IValidation>();
			var textErrors = wpfValidator.ErrorsFromBindings[nameof(ErrorClass.TextProperty)].OfType<IValidation>();

			Assert.That(numberErrors.Any());
			Assert.That(textErrors.Any());

		}
    }
}
