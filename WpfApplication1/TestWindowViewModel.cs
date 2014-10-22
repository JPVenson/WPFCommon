using JPB.ErrorValidation;
using JPB.ErrorValidation.ValidationRules;
using JPB.ErrorValidation.ValidationTyps;

namespace WpfApplication1
{
    public class TestWindowViewModel : AsyncErrorProviderBase<TestWindowViewModel, TestWindowViewModelRules>
    {
        public TestWindowViewModel()
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
            var vc_attributes = 0;
            Add(new Error<TestWindowViewModel>("Is null or empty", "ToValidationString", s => string.IsNullOrEmpty(s.ToValidationString)));
            Add(new Error<TestWindowViewModel>("Must be Int", "ToValidationString", s => !int.TryParse(s.ToValidationString, out vc_attributes)));
            Add(new Error<TestWindowViewModel>("Is too big", "ToValidationString", s => s.ToValidationString != null && s.ToValidationString.Length > 10));
        }
    }
}
