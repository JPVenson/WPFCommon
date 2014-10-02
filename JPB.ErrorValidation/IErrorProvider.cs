using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public interface IErrorProvider<T> : ICollection<IValidation<T>>
    {
        bool HasError { get; }
        bool WarningAsFailure { get; set; }
        //ICollection<IValidation<T>> VallidationErrors { get; }
        ObservableCollection<IValidation<T>> Errors { get; }
        IEnumerable<IValidation<T>> Warnings { get; }
        NoError<T> DefaultNoError { get; set; }

        Type RetrunT();
        IEnumerable<IValidation<T>> RetrunErrors(string columnName);
    }
}