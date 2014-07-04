using System.Collections.ObjectModel;
using IEADPC.DynamicInputBox.InfoWindow.Wrapper;

namespace IEADPC.DynamicInputBox.InfoWindow.Interfaces
{
    public interface IMultiQuestionViewModel<T> : IQuestionViewModel<T> where T : class
    {
        ObservableCollection<IListBoxItemWrapper> PossibleInput { get; set; }
        string ParsexQuestionText(object question);
    }
}