using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace JPB.ErrorValidation.ViewModelProvider.Base
{
    public abstract class SimpleErrorProviderBase<T> :
        SimpleErrorProviderBase where T : class, IErrorCollectionBase, new()
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

    public abstract class SimpleErrorProviderBase :
       ErrorProviderBase, IDataErrorInfo
    {
        protected SimpleErrorProviderBase(IErrorCollectionBase errors)
            : this(Dispatcher.FromThread(Thread.CurrentThread), errors)
        {

        }

        protected SimpleErrorProviderBase(Dispatcher dispatcher, IErrorCollectionBase errors)
            : base(dispatcher, errors)
        {
        }

        public new abstract string this[string columnName] { get; }
    }
}