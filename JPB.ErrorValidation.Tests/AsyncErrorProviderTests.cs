using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JPB.ErrorValidation.Tests
{
	[TestFixture]
	public class AsyncErrorProviderTests
	{
		public AsyncErrorProviderTests()
		{

		}

		[Test]
		public async Task TestDefaultValidation()
		{
			var validator = new ErrorClass();
			validator.UserErrors.Add(new Error<ErrorClass>("ErrorText.Empty", e => string.IsNullOrWhiteSpace(e.TextProperty),
				nameof(ErrorClass.TextProperty)));

			validator.LoadErrorMapperData();

			var textBinding = new BindingMock<ErrorClass>(validator, nameof(ErrorClass.TextProperty));

			validator.TextProperty = "";
			await textBinding.WaitAndValidate(true);

			validator.TextProperty = "D";
			await textBinding.WaitAndValidate(false);

			validator.TextProperty = "";
			await textBinding.WaitAndValidate(true);
		}

		[Test]
		public async Task TestExceptionValidation()
		{
			var validator = new ErrorClass();

			var textBinding = new BindingMock<ErrorClass>(validator, nameof(ErrorClass.TextProperty));
			
			await textBinding.WaitAndValidate(false);
			validator.TextProperty = "";
			
			await textBinding.WaitAndValidate(false);
			validator.TextProperty = "DASD";

			validator.UserErrors.Add(new Error<ErrorClass>("ErrorText.Empty", e => throw new Exception(),
				nameof(ErrorClass.TextProperty)));
			await textBinding.WaitAndValidate(false);

			await validator.ScheduleErrorUpdate(nameof(ErrorClass.TextProperty));
			await textBinding.WaitAndValidate(true);
		}

		[Test]
		public async Task TestMultiPropertyValidation()
		{
			var validator = new ErrorClass();

			var textBinding = new BindingMock<ErrorClass>(validator, nameof(ErrorClass.TextProperty));
			var numberBinding = new BindingMock<ErrorClass>(validator, nameof(ErrorClass.NumberProperty));

			validator.UserErrors.Add(new Error<ErrorClass>("NumberProperty.Zero&ErrorText.Empty", 
				e => e.NumberProperty == 0 || string.IsNullOrWhiteSpace(e.TextProperty),
				nameof(ErrorClass.TextProperty), nameof(ErrorClass.NumberProperty)));

			validator.TextProperty = "";
			await textBinding.WaitAndValidate(true);

			validator.TextProperty = "D";
			await textBinding.WaitAndValidate(true);

			validator.NumberProperty = 0;
			await textBinding.WaitAndValidate(true);

			validator.NumberProperty = 1;
			await textBinding.WaitAndValidate(false);
		}
	}
}