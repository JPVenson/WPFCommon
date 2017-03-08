namespace JPB.ErrorValidation.ValidationTyps
{
    public class AsyncValidationOption : IAsyncValidationOption
    {
        public AsyncState AsyncState { get; set; }
        public AsyncRunState RunState { get; set; }
    }
}