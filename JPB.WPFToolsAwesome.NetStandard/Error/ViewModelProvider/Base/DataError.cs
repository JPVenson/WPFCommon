using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using System.Xml.Serialization;
using JPB.ErrorValidation.ViewModelProvider;

namespace JPB.WPFToolsAwesome.Error.ViewModelProvider.Base
{
	/// <summary>
	///     Provides the IDataErrorInfo interface
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class DataError<T> : DataError, IDataErrorInfo where T : class, IErrorCollectionBase, new()
	{
		public DataError() : base(new T())
		{
		}

		public DataError(Dispatcher dispatcher)
			: base(dispatcher, new T())
		{
		}
	}

	/// <summary>
	///     Provides the IDataErrorInfo interface
	/// </summary>
	public abstract class DataError : ErrorProviderBase, IDataErrorInfo
	{
		protected DataError(IErrorCollectionBase errors) : base(errors)
		{
		}

		protected DataError(Dispatcher dispatcher, IErrorCollectionBase errors)
			: base(dispatcher, errors)
		{
		}

		/// <summary>
		///     For IDataErrorInfo support with multiple Errors for one field.
		/// </summary>
		[Browsable(false)]
		[XmlIgnore]
		public Func<object, object, object> AggregateMultiError { get; set; }

		public new string this[string columnName]
		{
			get
			{
				var validation = GetError(columnName, this);
				if (validation.Any())
				{
					return (Error = validation.Select(e => e.ErrorText).Aggregate(AggregateMultiError)?.ToString());
				}

				return string.Empty;
			}
		}

		public string Error { get; set; }
	}
}