using System.Collections.Generic;
using System.Collections.Specialized;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation
{
	public interface IErrorCollectionBase : ICollection<IValidation>, INotifyCollectionChanged
	{
		IEnumerable<IValidation> FilterErrors(string columnName);
	}
}