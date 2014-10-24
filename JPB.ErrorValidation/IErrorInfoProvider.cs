using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public interface IErrorInfoProvider<T> : ICollection<IValidation<T>>
    {
        bool HasError { get; }
        bool WarningAsFailure { get; set; }

        ThreadSaveObservableCollection<IValidation<T>> Errors { get; }
        IEnumerable<IValidation<T>> Warnings { get; }

        Type RetrunT();
        IEnumerable<IValidation<T>> RetrunErrors(string columnName);
    }
}