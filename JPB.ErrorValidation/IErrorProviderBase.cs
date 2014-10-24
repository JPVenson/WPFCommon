using System;
using System.ComponentModel;
using System.Linq.Expressions;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation
{
    public interface IErrorProviderBase<T> : 
        INotifyPropertyChanged where T : class
    {
        bool HasError { get; }
        bool AddTypeToText { get; set; }
        IErrorInfoProvider<T> ErrorInfoProviderSimpleAccessAdapter { get; }

        void ForceRefresh();
        string GetError(string columnName, T obj);
    }
}