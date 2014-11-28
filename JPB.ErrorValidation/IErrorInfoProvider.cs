using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public interface IErrorInfoProvider<T> : ICollection<IValidation<T>>, INotifyCollectionChanged
    {
        bool HasError { get; }
        bool WarningAsFailure { get; set; }

        ICollection<IValidation<T>> Errors { get; }
        IEnumerable<IValidation<T>> Warnings { get; }

        Type RetrunT();
        IEnumerable<IValidation<T>> RetrunErrors(string columnName);
    }
}