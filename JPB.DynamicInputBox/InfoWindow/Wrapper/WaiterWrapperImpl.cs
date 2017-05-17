using System;

namespace JPB.DynamicInputBox.InfoWindow.Wrapper
{
    public class WaiterWrapperImpl<T> : WaiterWrapper<T>
    {
        public WaiterWrapperImpl(Func<T> workerFunction, string waiterText)
        {
            base.WaiterText = waiterText;
            base.WorkerFunction = workerFunction;
        }

        public override string ToString()
        {
            return WaiterText;
        }
    }

    class WaiterWrapperSimple : WaiterWrapperImpl<object>
    {
        public WaiterWrapperSimple(Func<object> workerFunction, string waiterText) 
            : base(workerFunction, waiterText)
        {
        }
    }
}