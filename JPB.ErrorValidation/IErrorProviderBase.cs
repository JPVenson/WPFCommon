using System;
using System.ComponentModel;
using System.Linq.Expressions;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation
{
    public interface IErrorProviderBase<T> : 
        //new
        //INotifyDataErrorInfo,
        IDataErrorInfo, 
        INotifyPropertyChanged where T : class
    {
        bool HasError { get; }
        bool AddTypeToText { get; set; }
        NoError<T> DefaultNoError { get; set; }
        IErrorProvider<T> ErrorProviderSimpleAccessAdapter { get; }

        void ForceRefresh();
        string GetError(string columnName, T obj);
    }
}