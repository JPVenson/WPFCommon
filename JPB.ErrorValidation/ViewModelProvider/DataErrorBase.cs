using System.Linq;
using System.Windows.Threading;
using JPB.ErrorValidation.ViewModelProvider.Base;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation.ViewModelProvider
{
    /// <summary>
    /// Provides the IDataErrorInfo interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataErrorBase<T> : DataErrorBase where T : class, IErrorCollectionBase, new()
    {
        public DataErrorBase() : base(new T())
        {

        }

        public DataErrorBase(Dispatcher dispatcher)
            : base(dispatcher, new T())
        {

        }
    }

    /// <summary>
    /// Provides the IDataErrorInfo interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataErrorBase : SimpleErrorProviderBase
    {
        protected DataErrorBase(IErrorCollectionBase errors):base(errors)
        {

        }

        protected DataErrorBase(Dispatcher dispatcher, IErrorCollectionBase errors)
            : base(dispatcher, errors)
        {

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

        public bool this[string columnOrTaskName, bool error]
        {
            get
            {
                if (error)
                {
                    return string.IsNullOrWhiteSpace(this[columnOrTaskName]);
                }
                return ((AsyncViewModelBase)this)[columnOrTaskName];
            }
        }
    }
}