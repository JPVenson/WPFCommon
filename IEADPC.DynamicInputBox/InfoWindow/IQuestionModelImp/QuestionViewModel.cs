﻿#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 10:48

#endregion

using IEADPC.ErrorValidation.ValidationRules;

namespace IEADPC.DynamicInputBox.InfoWindow.IQuestionModelImp
{
    public class QuestionViewModel : QuestionAbstrViewModelBase<QuestionViewModel, QuestionViewModelValidation>
    {
        public QuestionViewModel(object question, EingabeModus eingabeModus)
            : base(question, eingabeModus)
        {
        }
    }

    public class QuestionViewModelValidation : ValidationRuleBase<QuestionViewModel>
    {
    }
}