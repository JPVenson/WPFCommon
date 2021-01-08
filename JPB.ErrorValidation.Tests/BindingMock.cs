using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JPB.WPFToolsAwesome.Error.ViewModelProvider.Base;
using NUnit.Framework;

namespace JPB.ErrorValidation.Tests
{
	public class BindingMock<T> where T : AsyncErrorProviderBase
	{
		private readonly T _viewModel;
		private readonly string _bindingPath;

		public BindingMock(T viewModel, string bindingPath)
		{
			_viewModel = viewModel;
			_bindingPath = bindingPath;
			_viewModel.ErrorsChanged += _viewModel_ErrorsChanged;
			ValidationRulesErrors = _viewModel.GetErrors(_bindingPath).OfType<IValidation<T>>().ToArray();
		}

		private void _viewModel_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
		{
			if (e.PropertyName == _bindingPath)
			{
				ValidationRulesErrors = _viewModel.GetErrors(_bindingPath).OfType<IValidation<T>>().ToArray();
			}
		}

		public IEnumerable<IValidation<T>> ValidationRulesErrors { get; set; }

		public bool HasErrors
		{
			get
			{
				return ValidationRulesErrors.Any();
			}
		}

		public async Task WaitAndValidate(bool assertHasError)
		{
			await _viewModel.AwaitRunningValidation();
			await WpfBootstrapper.WaitForDispatcher();
			Assert.That(HasErrors, Is.EqualTo(assertHasError));
		}
	}
}