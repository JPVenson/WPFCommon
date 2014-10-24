#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:48

#endregion

using System.Threading;
using System.Windows.Threading;
using JPB.ErrorValidation.ValidationRules;

namespace JPB.DynamicInputBox.InfoWindow.IQuestionModelImp
{
    public class QuestionViewModel : QuestionAbstrViewModelBase<QuestionViewModel, QuestionViewModelValidation>
    {
        public QuestionViewModel(object question, EingabeModus eingabeModus)
            : base(question, eingabeModus)
        {
            base.Dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
        }
    }

    public class QuestionViewModelValidation : ValidationRuleBase<QuestionViewModel>
    {
    }
}