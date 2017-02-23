using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public interface IErrorCollectionBase : ICollection<IValidation>
    {
        IEnumerable<IValidation> ReturnErrors(string columnName);
    }

    public interface IErrorValidatorBase
    {
        string Error { get; set; }
        bool Validate { get; set; }
        string MessageFormat { get; set; }
        bool HasError { get; }
        Type RetrunT();
        void Init();
        void ForceRefresh();
        IValidation[] GetError(string columnName, object obj);
        ICollection<IValidation> ActiveValidationCases { get; }
    }

    public interface IErrorValidatorBase<out T> : IErrorValidatorBase, INotifyCollectionChanged where T : IErrorCollectionBase
    {
        T UserErrors { get; }
    }
}