using System.Windows.Threading;

namespace JPB.ErrorValidation
{
    public class DataErrorBase<T, TE> : SimpleErrorProviderBase<T, TE> where T : class where TE : class, IErrorInfoProvider<T>, new()
    {
        public DataErrorBase()
        {
            
        }

        public DataErrorBase(Dispatcher dispatcher)
            : base(dispatcher)
        {
            
        }

        public override string this[string columnName]
        {
            get
            {
                var validation = base.GetError(columnName, this as T);
                if (validation != null)
                    return validation.ErrorText;
                return string.Empty;
            }
        }
    }
}