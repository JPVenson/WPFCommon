namespace JPB.WPFToolsAwesome.Error.ValidationTyps
{
    public class AsyncValidationOption : IAsyncValidationOption
    {
        public AsyncState AsyncState { get; set; }
        public AsyncRunState RunState { get; set; }
    }
}