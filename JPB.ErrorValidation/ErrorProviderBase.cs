using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using JPB.ErrorValidation.ValidationTyps;
using JPB.Tasking.TaskManagement.Threading;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public abstract class ErrorProviderBase<T, TE> :
        ErrorProviderBaseBase<T, TE>,
        IDataErrorInfo
        where T : class
        where TE : class, IErrorInfoProvider<T>, new()
    {
        protected ErrorProviderBase()
        {

        }

        #region IErrorProviderBase<T> Members

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                var validation = GetError(columnName, this as T);
                if (validation != null)
                return validation.ErrorText;
                return string.Empty;
            }
        }

        #endregion
    }
}