using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationTypes;

namespace JPB.WPFToolsAwesome.Error
{
	/// <summary>
	///		A Generic not WPF related Interface for a Validator
	/// </summary>
	public interface IErrorValidatorBase
	{
		/// <summary>
		/// Enabled/Disable all validation
		/// </summary>
		bool Validate { get; set; }
		
		/// <summary>
		/// The Errors that are used for validation
		/// </summary>
		IErrorCollectionBase UserErrors { get; }

		/// <summary>
		///		Replaces the underlying error collection
		/// </summary>
		/// <param name="userErrors"></param>
		/// <returns></returns>
		Task ReplaceUserErrorCollection(IErrorCollectionBase userErrors);

		/// <summary>
		/// Are any Errors known?
		/// </summary>
		bool HasError { get; }

		/// <summary>
		/// Refresh the Errors
		/// </summary>
		Task ForceRefreshAsync();

		/// <summary>
		/// Gets all Errors for the Field
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="validationObject"></param>
		/// <returns></returns>
		IValidation[] GetError(string fieldName, object validationObject);

		/// <summary>
		/// The list of all Active Errors
		/// </summary>
		ICollection<IValidation> ActiveValidationCases { get; }
	}
}