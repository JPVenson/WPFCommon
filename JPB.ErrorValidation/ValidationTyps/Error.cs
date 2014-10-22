using System;

namespace JPB.ErrorValidation.ValidationTyps
{
    /// <summary>
    ///     Endhält ein gültigen Error
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

        #region IValidation<T> Members

        public string[] ErrorIndicator { get; set; }

        public string ErrorText { get; set; }

        public Func<T, bool> Condition { get; set; }

        public string ErrorType
        {
            get { return "Need"; }
        }

        #endregion
    }
}