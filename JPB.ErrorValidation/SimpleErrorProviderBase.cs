using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using System.Xml.Serialization;
using JPB.ErrorValidation.ValidationTyps;
using JPB.Tasking.TaskManagement.Threading;
using JPB.WPFBase.MVVM.ViewModel;

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

        public abstract string this[string columnName] { get; }
    }
}