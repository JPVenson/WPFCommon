using System.Collections.Specialized;

namespace JPB.WPFToolsAwesome.Error
{
    public interface IErrorValidatorBase<out T> : IErrorValidatorBase, INotifyCollectionChanged where T : IErrorCollectionBase
    {
        /// <summary>
        /// The Errors that are used for validation
        /// </summary>
        new T UserErrors { get; }
    }
}