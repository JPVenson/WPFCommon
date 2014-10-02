using JPB.ErrorValidation;
using JPB.ErrorValidation.ValidationRules;
using JPB.ErrorValidation.ValidationTyps;

namespace WpfApplication1
{
    public class TestWindowViewModel : ErrorProviderBase<TestWindowViewModel, TestWindowViewModelRules>
    {

        public TestWindowViewModel()
            : base(1)
        {

        }

        private string _toValidationString;

        public string ToValidationString
        {
            get { return _toValidationString; }
            set
            {
                _toValidationString = value;
                SendPropertyChanged(() => ToValidationString);
            }
        }
    }

    public class TestWindowViewModelRules : ValidationRuleBase<TestWindowViewModel>
    {
        public TestWindowViewModelRules()
        {
            Add(new Error<TestWindowViewModel>("Is null or empty", "ToValidationString", s => string.IsNullOrEmpty(s.ToValidationString)));
            Add(new Error<TestWindowViewModel>("Is too big", "ToValidationString", s => s.ToValidationString != null && s.ToValidationString.Length > 10));
            Add(new Warning<TestWindowViewModel>("Is OK", "ToValidationString", s => s.ToValidationString != null && s.ToValidationString.Length < 10));
        }
    }
}
