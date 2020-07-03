using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JPB.WPFToolsAwesome.Error.ValidationTyps;

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
		/// <param name="newCollection"></param>
		/// <returns></returns>
		Task ReplaceUserErrorCollection(IErrorCollectionBase newCollection);

		/// <summary>
		/// Are any Errors known?
		/// </summary>
		bool HasError { get; }

		/// <summary>
		/// Refresh the Errors
		/// </summary>
		[Obsolete("Please use the ForceRefreshAsync")]
		void ForceRefresh();

		/// <summary>
		/// Refresh the Errors
		/// </summary>
		Task ForceRefreshAsync();

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