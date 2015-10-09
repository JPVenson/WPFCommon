using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace JPB.ErrorValidation
{
    public abstract class SimpleErrorProviderBase<T, TE> :
        ErrorProviderBase<T, TE>,
        IDataErrorInfo
        where T : class
        where TE : class, IErrorInfoProvider<T>, new()
    {
        protected SimpleErrorProviderBase()
            : this(Dispatcher.FromThread(Thread.CurrentThread))
        {

        }

        protected SimpleErrorProviderBase(Dispatcher dispatcher)
            : base(dispatcher)
        {

        }

        public new abstract string this[string columnName] { get; }
    }
}