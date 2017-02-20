#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:48

#endregion

using System.Threading;
using System.Windows.Threading;
using JPB.ErrorValidation;
using JPB.ErrorValidation.ValidationRules;

namespace JPB.DynamicInputBox.InfoWindow.IQuestionModelImp
{
    public class QuestionViewModel : QuestionAbstrViewModelBase<QuestionViewModelValidation>
    {
        public QuestionViewModel(object question, InputMode inputMode)
            : base(question, inputMode)
        {

        }
    }

    public class QuestionViewModelValidation : ErrorCollection<QuestionViewModel>
    {
    }
}