using System;
using System.ComponentModel;
using System.Linq.Expressions;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation
{
    public interface IErrorProviderBase
    {
        string Error { get; set; }
        bool Validate { get; set; }
        bool HasError { get; }
        string MessageFormat { get; set; }
        bool HasErrors { get; }
        void Init();
        void ForceRefresh();
    }

    public interface IErrorProviderBase<T> : 
        INotifyPropertyChanged where T : class
    {
        bool HasError { get; }
        string MessageFormat { get; set; }
        IErrorInfoProvider<T> ErrorInfoProviderSimpleAccessAdapter { get; }

        void ForceRefresh();
        string GetError(string columnName, T obj);
    }
}