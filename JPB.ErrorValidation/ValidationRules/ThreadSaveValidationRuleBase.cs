using System;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Threading;
using JPB.ErrorValidation.ValidationTyps;

namespace JPB.ErrorValidation.ValidationRules
{
    public class ThreadSaveValidationRuleBase : ErrorCollection
    {
        public ThreadSaveValidationRuleBase(Type validationType) : base(validationType)
        {
            Errors = new ThreadSaveObservableCollection<IValidation>(Dispatcher.FromThread(Thread.CurrentThread));
        }

        public new ThreadSaveObservableCollection<IValidation> Errors
        {
            get;
            set;
        }

        public new event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { Errors.CollectionChanged += value; }
            remove { Errors.CollectionChanged -= value; }
        }
    }
}