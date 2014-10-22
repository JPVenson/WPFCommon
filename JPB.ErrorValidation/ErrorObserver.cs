namespace JPB.ErrorValidation
{
    public class ErrorObserver<T>
    {
        #region SINGELTON for ever

        private static ErrorObserver<T> _instance;

        private ErrorObserver()
        {
        }

        public static ErrorObserver<T> Instance
        {
            get { return _instance ?? (_instance = new ErrorObserver<T>()); }
            set { _instance = value; }
        }

        #endregion

        private IErrorInfoProvider<T> InfoProvider { get; set; }

        public void RegisterErrorProvider(IErrorInfoProvider<T> infoProvider)
        {
            InfoProvider = (infoProvider);
        }

        public void UnregistErrorProvider(IErrorInfoProvider<T> infoProvider)
        {
            InfoProvider = null;
        }

        public IErrorInfoProvider<T> GetProviderViaType()
        {
            return InfoProvider;
        }
    }
}