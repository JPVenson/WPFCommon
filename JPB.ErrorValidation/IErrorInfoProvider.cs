using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public interface IErrorValidatorBase<out T> : IErrorValidatorBase, INotifyCollectionChanged where T : IErrorCollectionBase
    {
        /// <summary>
        /// The Errors that are used for validation
        /// </summary>
        new T UserErrors { get; }
    }
}