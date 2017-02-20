using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace JPB.ErrorValidation.ViewModelProvider.Base
{
    public abstract class SimpleErrorProviderBase<T> :
        ErrorProviderBase<T>,
        IDataErrorInfo where T : class, IErrorCollectionBase, new()
    {
        protected SimpleErrorProviderBase()
            : this(Dispatcher.FromThread(Thread.CurrentThread))
        {

        }

        protected SimpleErrorProviderBase(Dispatcher dispatcher)
            : base(dispatcher)
        {
            AggregateMultiError = (s, s1) => s + Environment.NewLine + s1;
        }

        public new abstract string this[string columnName] { get; }
    }
}