using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using JPB.ErrorValidation.ValidationTyps;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
    public interface IErrorCollectionBase : ICollection<IValidation>, INotifyCollectionChanged
    {
        IEnumerable<IValidation> ReturnErrors(string columnName);
    }

    public interface IErrorValidatorBase
    {
        /// <summary>
        /// The general Error string for this Object
        /// </summary>
        string Error { get; set; }
        /// <summary>
        /// Enabled/Disable all validation
        /// </summary>
        bool Validate { get; set; }
        /// <summary>
        /// if and how messages should be formatted
        /// </summary>
        string MessageFormat { get; set; }
        /// <summary>
        /// The Errors that are used for validation
        /// </summary>
        IErrorCollectionBase UserErrors { get; set; }
        /// <summary>
        /// Are any Errors known?
        /// </summary>
        bool HasError { get; }
        /// <summary>
        /// Refresh the Errors
        /// </summary>
        void ForceRefresh();
        /// <summary>
        /// Gets all Errors for the Field
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        IValidation[] GetError(string columnName, object obj);
        /// <summary>
        /// The list of all Active Errors
        /// </summary>
        ICollection<IValidation> ActiveValidationCases { get; }
    }

    public interface IErrorValidatorBase<out T> : IErrorValidatorBase, INotifyCollectionChanged where T : IErrorCollectionBase
    {
        /// <summary>
        /// The Errors that are used for validation
        /// </summary>
        new T UserErrors { get; }
    }
}