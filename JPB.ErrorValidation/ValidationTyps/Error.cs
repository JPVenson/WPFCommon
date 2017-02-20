using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation.ValidationTyps
{
    /// <summary>
    ///     Defines an Error
    /// </summary>
    public class Error<T> : IValidation<T>
    {
        public Error(string errorText, string errorIndicator, Func<T, bool> condition)
        {
            this.Condition = condition;
            this.ErrorIndicator = new[] { errorIndicator };
            this.ErrorText = errorText;
        }

        public Error(string errorText, Func<T, bool> condition, params string[] errorIndicator)
        {
            this.Condition = condition;
            this.ErrorIndicator = errorIndicator;
            this.ErrorText = errorText;
        }

        public static Error<T> FromProperty<TProperty>(string errorText, Func<T, bool> condition,
            Expression<Func<TProperty>> errorIndicator)
        {
            return new Error<T>(errorText, condition, ViewModelBase.GetPropertyName(errorIndicator));
        }

        public Error<T> Include<TProperty>(Expression<Func<TProperty>> errorIndicator)
        {
            ErrorIndicator = ErrorIndicator.Concat(new[] { ViewModelBase.GetPropertyName(errorIndicator) }).ToArray();
            return this;
        }

        /// <summary>
        /// Appends an Error if this Condition was True
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public List<IValidation<T>> And(Error<T> error)
        {
            var oldCondition = error.Condition;
            error.Condition = (obj) =>
            {
                if (Condition(obj))
                {
                    return oldCondition(obj);
                }
                return false;
            };
            return new List<IValidation<T>>()
            {
                this, error
            };
        }

        #region IValidation<T> Members

        public string[] ErrorIndicator { get; set; }

        public string ErrorText { get; set; }

        public Func<T, bool> Condition { get; set; }

        public string ErrorType
        {
            get { return "Need"; }
        }

        /// <summary>
        /// The Condition that indicates an Error. True error, False NoError
        /// </summary>
        Func<object, bool> IValidation.Condition
        {
            get
            {
                return (validator) =>
                {
                    if (!(validator is T))
                        return true;
                    return Condition((T)validator);
                };
            }
        }

        #endregion
    }

    //public class IntCheck<T, TProperty> : Error<T>
    //{
    //    public IntCheck(Expression<Func<TProperty>> errorIndicator) : base("The field does not contain a Number", ViewModelBase.GetPropertyName(errorIndicator),
    //        obj =>
    //        {
    //            var propInfo = ViewModelBase.GetProperty(errorIndicator);
    //            var value = propInfo.GetValue(obj);
    //            if (value == null)
    //                return true;
    //            if (!(value is string))
    //                return true;
    //            int result;
    //            return int.TryParse((string)value, out result);
    //        })
    //    {

    //    }
    //}
}