using System;
using System.Collections.Generic;
using JPB.WPFToolsAwesome.Error.ValidationTyps;

namespace JPB.WPFToolsAwesome.Error.ValidationRules
{
	/// <summary>
	///     Defines a simple Collection of Errors where duplicates can not occur
	/// </summary>
	public abstract class ErrorHashSet : ErrorCollectionWrapper
	{
		public ErrorHashSet(Type validationType) : base(validationType)
		{
			Errors = new HashSet<IValidation>();
		}

		protected override ICollection<IValidation> Errors { get; }
	}
}