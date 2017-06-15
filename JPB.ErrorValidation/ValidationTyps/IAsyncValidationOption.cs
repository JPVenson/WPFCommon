﻿namespace JPB.ErrorValidation.ValidationTyps
{
    public interface IAsyncValidationOption
    {
        /// <summary>
        /// Should this error executed Async or sync
        /// </summary>
        AsyncState AsyncState { get; set; }
        /// <summary>
        /// Should only a Validation only executed when no other Validation of this type is executed by now
        /// </summary>
        AsyncRunState RunState { get; set; }
    }
}