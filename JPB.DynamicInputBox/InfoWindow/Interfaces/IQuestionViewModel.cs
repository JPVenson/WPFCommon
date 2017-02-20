#region Jean-Pierre Bachmann

// Erstellt von Jean-Pierre Bachmann am 15:28

#endregion

using System;
using JPB.ErrorValidation;

namespace JPB.DynamicInputBox.InfoWindow.Interfaces
{
    public interface IQuestionViewModel<T>
        where T : class
    {
        bool IsInit { get; set; }
        Object Question { get; set; }
        Object Input { get; set; }
        InputMode SelectedInputMode { get; set; }
        void Init();
    }
}