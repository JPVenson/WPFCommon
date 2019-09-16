using System;
using System.Threading.Tasks;

namespace JPB.ErrorValidation.ValidationTyps
{
    /// <summary>
    ///     Defines a Error to be executed Async
    /// </summary>
    public interface IAsyncValidation : IValidation, IAsyncValidationOption
    {
	    /// <summary>
	    ///		The Condition that indicates an Error. True error, False NoError
	    /// </summary>
	    Func<object, Task<bool>> AsyncCondition { get; set; }
	}

    /// <summary>
    ///     Defines a Error to be executed Async
    /// </summary>
    public interface IAsyncValidation<T> : IAsyncValidation
	{
	    /// <summary>
	    ///		The Condition that indicates an Error. True error, False NoError
	    /// </summary>
	    new Func<T, Task<bool>> AsyncCondition { get; set; }
	}
}