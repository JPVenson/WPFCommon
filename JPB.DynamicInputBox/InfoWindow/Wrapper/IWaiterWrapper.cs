using System;

namespace JPB.DynamicInputBox.InfoWindow.Wrapper
{
    public interface IWaiterWrapper<T>
    {
        bool IsAsnc { get; set; }
        Func<T> WorkerFunction { get; set; }
        string WaiterText { get; set; }
        int MaxProgress { get; set; }
        int CurrentProgress { get; set; }
    }
}