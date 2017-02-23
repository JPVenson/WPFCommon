using System.Linq;
using System.Windows.Threading;
using JPB.ErrorValidation.ViewModelProvider.Base;

namespace JPB.ErrorValidation.ViewModelProvider
{
    /// <summary>
    /// Provides the IDataErrorInfo interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataErrorBase<T> : SimpleErrorProviderBase<T> where T : class, IErrorCollectionBase, new()
    {
        public DataErrorBase()
        {

        }

        public DataErrorBase(Dispatcher dispatcher)
            : base(dispatcher)
        {

        }

        public string GetError(string columnName, T obj)
        {
            base.GetError(columnName, obj);
            return Error;
        }

        public override string this[string columnName]
        {
            get
            {
                var validation = base.GetError(columnName, this);
                if (validation.Any())
                    return Error;
                return string.Empty;
            }
        }
    }
}