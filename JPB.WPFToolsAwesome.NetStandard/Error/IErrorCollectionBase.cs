using System.Collections.Generic;
using System.Collections.Specialized;
using JPB.WPFToolsAwesome.Error.ValidationTyps;

namespace JPB.WPFToolsAwesome.Error
{
	public interface IErrorCollectionBase : ICollection<IValidation>, INotifyCollectionChanged
	{
		IEnumerable<IValidation> FilterErrors(string columnName);
	}
}