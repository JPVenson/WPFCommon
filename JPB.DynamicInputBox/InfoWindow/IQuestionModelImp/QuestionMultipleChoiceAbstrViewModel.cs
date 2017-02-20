#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 11:00

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JPB.DynamicInputBox.InfoWindow.Wrapper;
using JPB.ErrorValidation;
using JPB.ErrorValidation.ValidationRules;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.DynamicInputBox.InfoWindow.IQuestionModelImp
{
    public abstract class QuestionMultipleChoiceAbstrViewModel : QuestionAbstrViewModelBase<QuestionMultiChoiceViewModelValid>
    {
        protected QuestionMultipleChoiceAbstrViewModel(object question, InputMode inputMode)
            : base(question, inputMode)
        {
            Output = new ObservableCollection<IListBoxItemWrapper>();
        }

        #region ListBoxItemWrappers property

        public ObservableCollection<IListBoxItemWrapper> Output
        {
            get { return Input as ObservableCollection<IListBoxItemWrapper>; }
            set
            {
                Input = value;
                SendPropertyChanged(() => Output);
            }
        }

        #endregion

        #region PossibleInput property

        private ObservableCollection<IListBoxItemWrapper> _possibleInput;

        public ObservableCollection<IListBoxItemWrapper> PossibleInput
        {
            get { return _possibleInput; }
            set
            {
                _possibleInput = value;
                SendPropertyChanged(() => PossibleInput);
            }
        }

        #endregion
    }

    public class QuestionMultiChoiceViewModelValid : ErrorCollection<QuestionMultipleChoiceAbstrViewModel>
    {
        public QuestionMultiChoiceViewModelValid()
        {
            Add(new Error<QuestionMultipleChoiceAbstrViewModel>(
                "Bitte wähle mindestens ein Item aus", "Input", s =>
                {
                    if (s.Input == null)
                        return true;
                    if (s.Input is List<ListBoxItemWrapper>)
                    {
                        if (!(s.Input as List<ListBoxItemWrapper>).Any())
                            return true;
                    }
                    return false;
                }));
        }
    }
}