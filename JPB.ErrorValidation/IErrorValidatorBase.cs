using System.Collections.Generic;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation
{
	/// <summary>
	///		A Generic not WPF related Interface for a Validator that can be loaded into the 
	/// </summary>
	public interface IErrorValidatorBase
	{
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
}