using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JPB.DynamicInputBox.InfoWindow.Wrapper;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.DynamicInputBox.InfoWindow.IQuestionModelImp
{
    public class QuestionMutliOrSingelInputViewModel : QuestionMultipleChoiceAbstrViewModel
    {
        public QuestionMutliOrSingelInputViewModel(object question, InputMode inputMode)
            : base(question, inputMode)
        {

        }

        public string ParsexQuestionText(object input)
        {
            var text = (input as string);

            if (input is ObservableCollection<IListBoxItemWrapper>)
            {
                var inputs = input as ObservableCollection<IListBoxItemWrapper>;
                text = inputs.First().Text;
                inputs.RemoveAt(0);
                PossibleInput = inputs;
            }
            else
            {
                if (!(input is string))
                    throw new ArgumentException("Can not parse text!");

                string lowertext = text.ToLower();
                if (!lowertext.Contains("#q"))
                    throw new ArgumentException("Can not parse text!");
                List<string> allquestions = text.Split(new[] { "#Q", "#q" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                text = allquestions.ElementAt(0);
                allquestions.RemoveAt(0);
                PossibleInput =
                    new ObservableCollection<IListBoxItemWrapper>(
                        allquestions.Select(
                            allquestion =>
                                new ListBoxItemWrapper { Text = allquestion, Index = allquestions.IndexOf(allquestion) }));
            }

            foreach (ListBoxItemWrapper item in PossibleInput)
            {
                item.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName != "IsChecked") return;

                    var listBoxItemWrapper = (sender as ListBoxItemWrapper);
                    if (listBoxItemWrapper.IsChecked)
                        Output.Add(listBoxItemWrapper);
                    else
                        Output.Remove(listBoxItemWrapper);

                    base.ForceRefresh();
                };
            }

            return text;
        }

        public override void Init()
        {
            Question = ParsexQuestionText(Question);
            base.Init();
        }
    }
}