using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using JPB.ErrorValidation.ValidationTyps;
using JPB.ErrorValidation.ViewModelProvider.Base;
using JPB.Tasking.TaskManagement.Threading;

namespace JPB.ErrorValidation.ViewModelProvider
{
    /// <summary>
    ///     Provides the INotifyDataErrorInfo Interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TE"></typeparam>
    public abstract class AsyncErrorProviderBase<T> :
        ErrorProviderBase<T>,
        INotifyDataErrorInfo where T : class, IErrorCollectionBase, new()
    {
        private readonly SingelSeriellTaskFactory _errorFactory = new SingelSeriellTaskFactory(2);
        private readonly object _lockRoot = new object();
        private int _workerCount;

        protected AsyncErrorProviderBase()
        {
            ErrorMapper = new ConcurrentDictionary<string, HashSet<IValidation>>();

            PropertyChanged += AsyncErrorProviderBase_PropertyChanged;

            foreach (var validation in UserErrors.SelectMany(f => f.ErrorIndicator).Distinct())
            {
                ErrorMapper.GetOrAdd(validation, new HashSet<IValidation>());
            }

            AsyncValidationOption = new AsyncValidationOption
            {
                AsyncState = AsyncState.AsyncSharedPerCall,
                RunState = AsyncRunState.CurrentPlusOne
            };
        }

        /// <summary>
        /// If an Rendering is Requested, the ui should use this
        /// </summary>
        protected Func<IValidation, object> ValidationToUiError { get; set; }

        /// <summary>
        ///     The Default execution if the Item is no IAsyncValidation
        /// </summary>
        protected virtual IAsyncValidationOption AsyncValidationOption { get; }

        /// <summary>
        ///     Is currently Validating
        /// </summary>
        public bool IsValidating
        {
            get { return _workerCount > 0; }
        }

        /// <summary>
        ///     Gets all Error lists for all collumns
        /// </summary>
        public ConcurrentDictionary<string, HashSet<IValidation>> ErrorMapper { get; }

        /// <inheritdoc />
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <inheritdoc />
        public IEnumerable GetErrors(string propertyName)
        {
            HashSet<IValidation> errorJob;
            ErrorMapper.TryGetValue(propertyName, out errorJob);
            return errorJob?.Select(f => ValidationToUiError == null ? f : ValidationToUiError(f));
        }

        /// <inheritdoc />
        public bool HasErrors
        {
            get { return HasError; }
        }

        public void ScheduleErrorUpdate<TProp>(Expression<Func<TProp>> property)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            ScheduleErrorUpdate(GetPropertyName(property));
        }

        /// <summary>
        /// Schedules a new Update for the Property
        /// </summary>
        /// <param name="propertyName"></param>
        public void ScheduleErrorUpdate([CallerMemberName] string propertyName = null)
        {
            if (propertyName != null && ErrorMapper.ContainsKey(propertyName))
                ValidateAsync(propertyName);
        }

        private void AsyncErrorProviderBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            ScheduleErrorUpdate(e.PropertyName);
        }

        /// <inheritdoc />
        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);
        }

        private void RunAsyncTask(Func<IValidation[]> toExection, AsyncRunState validation2Key,
            Action<IValidation[]> then,
            string key)
        {
            switch (validation2Key)
            {
                case AsyncRunState.NoPreference:
                    if (_errorFactory.ConcurrentQueue.Count >= Environment.ProcessorCount)
                    {
                        _errorFactory.Add(() =>
                        {
                            var exec = toExection();
                            BeginThreadSaveAction(() => { then(exec); });
                        }, key);
                    }
                    else
                    {
                        SimpleWorkWithSyncContinue(toExection, then);
                    }

                    break;
                case AsyncRunState.CurrentPlusOne:
                    _errorFactory.Add(() =>
                    {
                        var exec = toExection();
                        BeginThreadSaveAction(() => { then(exec); });
                    }, key);
                    break;
                case AsyncRunState.OnlyOnePerTime:
                    _errorFactory.Add(() =>
                    {
                        var exec = toExection();
                        BeginThreadSaveAction(() => { then(exec); });
                    }, key, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(validation2Key), validation2Key, null);
            }
        }

        private void HandleErrors(IValidation[] newErrors, IValidation[] processed)
        {
            var handler = new ErrorToFieldHandler();
            handler.ErrorMapper = ErrorMapper;

            foreach (var validation in newErrors)
            {
                handler.AddErrorToField(validation);
            }

            handler.HandleUneffected(newErrors, processed);

            lock (_lockRoot)
            {
                _workerCount -= processed.Length;
            }

            BeginThreadSaveAction(() =>
            {
                SendPropertyChanged(() => IsValidating);
                foreach (var listOfValidator in processed.SelectMany(f => f.ErrorIndicator).Distinct())
                {
                    OnErrorsChanged(new DataErrorsChangedEventArgs(listOfValidator));
                }
            });
        }

        private void RunAsyncSharedPerCall(IValidation[] validation,
            AsyncRunState validation2Key)
        {
            RunAsyncTask(() => ObManage(validation, this), validation2Key,
                validations => { HandleErrors(validations, validation); },
                validation.Select(f => f.GetHashCode().ToString()).Aggregate((e, f) => e + f));
        }

        private void RunSync(IValidation[] validation)
        {
            HandleErrors(ObManage(validation, this), validation);
        }

        private void RunSyncToDispatcher(IValidation[] validation)
        {
            BeginThreadSaveAction(() => { RunSync(validation); });
        }

        private void RunAuto(IValidation[] validation, AsyncRunState validation2Key)
        {
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                RunAsyncSharedPerCall(validation, validation2Key);
            }
            else
            {
                RunSync(validation);
            }
        }

        private void HandleErrorEnumeration(IEnumerable<IValidation> elements, AsyncState asyncState,
            AsyncRunState asyncRunState)
        {
            switch (asyncState)
            {
                case AsyncState.AsyncSharedPerCall:
                {
                    RunAsyncSharedPerCall(elements.ToArray(), asyncRunState);
                    break;
                }
                case AsyncState.Sync:
                {
                    RunSync(elements.ToArray());
                    break;
                }
                case AsyncState.SyncToDispatcher:
                {
                    RunSyncToDispatcher(elements.ToArray());
                    break;
                }
                case AsyncState.NoPreference:
                {
                    RunAuto(elements.ToArray(), asyncRunState);
                    break;
                }
                case AsyncState.Async:
                {
                    foreach (var runIndipendendAsync in elements)
                    {
                        RunAsyncSharedPerCall(new[] {runIndipendendAsync}, asyncRunState);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ValidateAsync(string propertyName)
        {
            HashSet<IValidation> errorJob;

            if (!ErrorMapper.TryGetValue(propertyName, out errorJob))
            {
                return;
            }
            errorJob.Clear();

            var errors = ProduceValidations(propertyName).ToList();
            lock (_lockRoot)
            {
                _workerCount += errors.Count;
            }

            BeginThreadSaveAction(() => SendPropertyChanged(() => IsValidating));

            var nonAsync = errors.Where(f => !(f is IAsyncValidation)).ToArray();
            if (nonAsync.Any())
            {
                HandleErrorEnumeration(nonAsync, AsyncValidationOption.AsyncState, AsyncValidationOption.RunState);
            }

            foreach (
                var validation in
                errors.Where(f => f is IAsyncValidation).Cast<IAsyncValidation>().GroupBy(f => f.AsyncState))
            {
                foreach (var validation2 in validation.GroupBy(f => f.RunState))
                {
                    HandleErrorEnumeration(validation2, validation.Key, validation2.Key);
                }
            }
        }

        private class ErrorToFieldHandler
        {
            public ConcurrentDictionary<string, HashSet<IValidation>> ErrorMapper { get; set; }

            public List<HashSet<IValidation>> Effected { get; set; }

            public void AddErrorToField(IValidation error)
            {
                Effected = new List<HashSet<IValidation>>();
                foreach (var indicator in error.ErrorIndicator)
                {
                    var field = ErrorMapper.GetOrAdd(indicator, new HashSet<IValidation>());
                    field.Add(error);
                    Effected.Add(field);
                }
            }

            public void HandleUneffected(IValidation[] newErrors, IValidation[] processed)
            {
                foreach (var item in processed.Where(f => !newErrors.Contains(f)))
                {
                    foreach (var indicators in item.ErrorIndicator)
                    {
                        var field = ErrorMapper.GetOrAdd(indicators, new HashSet<IValidation>());
                        field.Remove(item);
                    }
                }
            }
        }
    }
}