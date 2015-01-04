#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:49

#endregion

using System.Threading;
using System.Windows.Threading;
using JPB.DynamicInputBox.InfoWindow.Interfaces;
using JPB.ErrorValidation;
using JPB.ErrorValidation.ValidationRules;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.DynamicInputBox.InfoWindow.IQuestionModelImp
{
    public interface IQuestionAbstrViewModelBase : IErrorProviderBase
    {
        bool IsInit { get; set; }
        object Question { get; set; }
        object Input { get; set; }
        InputMode SelectedInputMode { get; set; }
        new void Init();
    }

    public abstract class QuestionAbstrViewModelBase<T, TE> : ErrorProviderBase<T, TE>,
        IQuestionViewModel<T>,
        IQuestionAbstrViewModelBase
        where T : class
        where TE : class, IErrorInfoProvider<T>, new()
    {
        private object _input;
        private bool _isInit;
        private object _question;
        private InputMode _selectedInputMode;

        protected QuestionAbstrViewModelBase(object question, InputMode inputMode)
            : base(Dispatcher.FromThread(Thread.CurrentThread))
        {
            SelectedInputMode = inputMode;
            Question = question;
        }

        #region Implementation of IQuestionViewModel

        public bool IsInit
        {
            get { return _isInit; }
            set
            {
                _isInit = value;
                SendPropertyChanged(() => IsInit);
            }
        }

        public object Question
        {
            get { return _question; }
            set
            {
                _question = value;
                SendPropertyChanged(() => Question);
            }
        }

        public object Input
        {
            get { return _input; }
            set
            {
                _input = value;
                SendPropertyChanged(() => Input);
            }
        }

        public InputMode SelectedInputMode
        {
            get { return _selectedInputMode; }
            set
            {
                _selectedInputMode = value;
                SendPropertyChanged(() => SelectedInputMode);
            }
        }

        //public bool HasError { get; set; }

        public virtual void Init()
        {
            IsInit = true;
        }

        #endregion

        string IErrorProviderBase<T>.GetError(string columnName, T obj)
        {
            var validation = base.GetError(columnName, obj);
            if (validation != null)
                return validation.ErrorText;
            return string.Empty;
        }
    }

    public class QuestionAbstrViewModelBaseValidation<T, TE> : ValidationRuleBase<QuestionAbstrViewModelBase<T, TE>>
        where T : class, new()
        where TE : class, IErrorInfoProvider<T>, new()
    {
        public QuestionAbstrViewModelBaseValidation()
        {
            Add(new Error<QuestionAbstrViewModelBase<T, TE>>("Bitte gebe etwas in das Eingabefeld ein", "Input",
                s => s.Input == null));
        }
    }
}