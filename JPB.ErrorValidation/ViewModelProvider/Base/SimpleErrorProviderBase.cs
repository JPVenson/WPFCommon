using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace JPB.ErrorValidation.ViewModelProvider.Base
{
    public abstract class SimpleErrorProviderBase<T> :
        ErrorProvider where T : class, IErrorCollectionBase, new()
    {
        protected SimpleErrorProviderBase()
            : this(Dispatcher.FromThread(Thread.CurrentThread), new T())
        {

        }

        protected SimpleErrorProviderBase(Dispatcher dispatcher, T errors)
            : base(dispatcher, errors)
        {

        }
    }

    public abstract class ErrorProvider :
       ErrorProviderBase, IDataErrorInfo
    {
        protected ErrorProvider(IErrorCollectionBase errors)
            : this(Dispatcher.FromThread(Thread.CurrentThread), errors)
        {

        }

        protected ErrorProvider(Dispatcher dispatcher, IErrorCollectionBase errors)
            : base(dispatcher, errors)
        {
        }

        public new abstract string this[string columnName] { get; }
        public string Error { get; }
    }
}