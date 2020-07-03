using System;
using System.Threading;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel
{
    public class ActionWaiter
    {
        public ActionWaiter(Action action)
        {
            Action = action;
            Waiter = new ManualResetEventSlim();
        }
        public Action Action { get; private set; }
        public ManualResetEventSlim Waiter { get; private set; }
    }
}
