using System.Threading;
using JPB.ErrorValidation;
using JPB.ErrorValidation.ValidationRules;
using JPB.ErrorValidation.ValidationTyps;
using JPB.ErrorValidation.ViewModelProvider;
using JPB.WPFBase.MVVM.DelegateCommand;

namespace WpfApplication1
{
    public class TestWindowViewModel : DataErrorBase<TestWindowViewModelRules>
    {
        public TestWindowViewModel()
        {
            TaskACommand = new AsyncDelegateCommand(ExecuteTaskA, CanExecuteTaskA);
            TaskBCommand = new AsyncDelegateCommand(ExecuteTaskB, CanExecuteTaskB);
            TaskBCommand.AsyncCanExecute = true;
            TaskACommand.AsyncCanExecute = true;

            TaskACommand.AddDependency(TaskBCommand, DependLevel.Complete);
            TaskBCommand.AddDependency(TaskACommand, DependLevel.WorkingOnly);
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

        public AsyncDelegateCommand TaskBCommand { get; private set; }

        public void ExecuteTaskB(object sender)
        {
            Thread.CurrentThread.Join(10000);
            ToValidationString = 1337.ToString();
        }

        public bool CanExecuteTaskB(object sender)
        {
            Thread.CurrentThread.Join(2000);
            return true;
        }

        public AsyncDelegateCommand TaskACommand { get; private set; }

        public void ExecuteTaskA(object sender)
        {
            Thread.CurrentThread.Join(10000);
        }

        public bool CanExecuteTaskA(object sender)
        {
            return true;
        }
    }

    public class TestWindowViewModelRules : ErrorCollection<TestWindowViewModel>
    {
        public TestWindowViewModelRules()
        {
            var run = 0;
            var vc_attributes = 0;
            Add(new Error<TestWindowViewModel>("Is null or empty", "ToValidationString", s => string.IsNullOrEmpty(s.ToValidationString)));
            Add(new Error<TestWindowViewModel>("Is too big", "ToValidationString",
                s => s.ToValidationString != null && s.ToValidationString.Length > 5));
            Add(new Error<TestWindowViewModel>("Must be Int", "ToValidationString", s => !int.TryParse(s.ToValidationString, out vc_attributes)));
            //Add(new Error<TestWindowViewModel>("Wait", "ToValidationString", s =>
            //{
            //    Thread.Sleep(1000);
            //    run++;
            //    return run % 2 == 1;
            //}));
        }
    }
}
