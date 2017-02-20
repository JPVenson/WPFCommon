using System;

namespace JPB.ErrorValidation.ValidationTyps
{
    public interface IValidation
    {
        /// <summary>
        /// An description what kind of Validation is used e.g "Warning", "Error", "FYI"
        /// </summary>
        string ErrorType { get; }
        /// <summary>
        /// An indicator for all fields that participate on the Validation
        /// </summary>
        string[] ErrorIndicator { get; set; }
        /// <summary>
        /// The text that should be emitted when the Condition is True
        /// </summary>
        string ErrorText { get; set; }
        /// <summary>
        /// The Condition that indicates an Error. True error, False NoError
        /// </summary>
        Func<object, bool> Condition { get; }
    }

    public interface IValidation<in T> : IValidation
    {
        /// <summary>
        /// The Condition that indicates an Error. True error, False NoError
        /// </summary>
        new Func<T, bool> Condition { get; }
    }
}