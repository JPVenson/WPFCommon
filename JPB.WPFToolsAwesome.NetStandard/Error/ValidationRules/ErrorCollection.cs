using System;
using System.Collections.Generic;
using JPB.WPFToolsAwesome.Error.ValidationTyps;

namespace JPB.WPFToolsAwesome.Error.ValidationRules
{
	/// <summary>
	///		Defines a collection that is left empty
	/// </summary>
	public class NoErrors : ErrorCollection<object>
	{
	}

	/// <summary>
	///     Defines a simple Collection of Errors where duplicates can occur
	/// </summary>
	public class ErrorCollection : ErrorCollectionWrapper
	{
		protected ErrorCollection(Type validationType) : base(validationType)
		{
			Errors = new List<IValidation>();
		}

		protected override ICollection<IValidation> Errors { get; }
	}

	/// <summary>
	///     Defines a simple Collection of Errors where duplicates can occur
	/// </summary>
	public abstract class ErrorCollection<T> : ErrorCollection
	{
		public ErrorCollection()
			: base(typeof(T))
		{
		}
	}

	/// <summary>
	///     Defines a simple Collection of Errors where duplicates can not occur
	/// </summary>
	public abstract class ErrorHashSet<T> : ErrorCollection<T>
	{
		public ErrorHashSet()
		{
			Errors = new HashSet<IValidation>();
		}

		protected override ICollection<IValidation> Errors { get; }
	}
}