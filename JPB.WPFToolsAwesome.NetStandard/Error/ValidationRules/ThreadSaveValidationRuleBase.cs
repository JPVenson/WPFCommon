﻿using System;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Threading;
using JPB.WPFToolsAwesome.Error.ValidationTypes;
using JPB.WPFToolsAwesome.MVVM.ViewModel;

namespace JPB.WPFToolsAwesome.Error.ValidationRules
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