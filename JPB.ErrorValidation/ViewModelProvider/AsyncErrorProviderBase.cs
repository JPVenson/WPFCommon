using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
		private int _workerCount;
		private readonly object _lockRoot = new object();

		protected AsyncErrorProviderBase()
		{
			ErrorMapper = new ConcurrentDictionary<string, HashSet<IValidation>>();

			PropertyChanged += AsyncErrorProviderBase_PropertyChanged;

			foreach (var validation in UserErrors)
			{
				var errorList = new HashSet<IValidation>();

				foreach (var validationKey in validation.ErrorIndicator)
					ErrorMapper.GetOrAdd(validationKey, errorList);
			}
			AsyncValidationOption = new AsyncValidationOption
			{
				AsyncState = AsyncState.AsyncSharedPerCall,
				RunState = AsyncRunState.CurrentPlusOne
			};
		}

		protected Func<IValidation, object> ValidationToUiError { get; set; }

		/// <summary>
		///     The Default execution if the Item is no IAsyncValidation
		/// </summary>
		private IAsyncValidationOption AsyncValidationOption { get; set; }

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
		public ConcurrentDictionary<string, HashSet<IValidation>> ErrorMapper { get; private set; }

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		/// <summary>
		///     Gets all Errors for this Property
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public IEnumerable GetErrors(string propertyName)
		{
			ErrorMapper.TryGetValue(propertyName, out HashSet<IValidation> errorJob);
			return errorJob?.Select(f => ValidationToUiError == null ? f : ValidationToUiError(f));
		}

		/// <summary>
		/// Get the state of this Entity
		/// </summary>
		public bool HasErrors
		{
			get { return HasError; }
		}

		private void AsyncErrorProviderBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (ErrorMapper.ContainsKey(e.PropertyName))
				ValidateAsync(e.PropertyName);
		}

		protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
		{
			ErrorsChanged?.Invoke(this, e);
		}

		private void RunAsyncTask(Func<IValidation[]> toExection, AsyncRunState validation2Key, Action<IValidation[]> then,
			string key)
		{
			switch (validation2Key)
			{
				case AsyncRunState.NoPreference:
					SimpleWorkWithSyncContinue(toExection, then);
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

		private void HandleErrors(HashSet<IValidation> source, IValidation[] newErrors, IValidation[] processed)
		{
			foreach (var validation in newErrors)
			{
				source.Add(validation);
			}

			source.RemoveWhere(f => processed.Contains(f) && !newErrors.Contains(f));

			for (var i = 0; i < processed.Count(); i++)
			{
				lock (_lockRoot)
				{
					_workerCount -= _workerCount;
				}
			}

			BeginThreadSaveAction(() =>
			{
				SendPropertyChanged(() => IsValidating);
				foreach (var listOfValidator in processed.SelectMany(f => f.ErrorIndicator).Distinct())
					OnErrorsChanged(new DataErrorsChangedEventArgs(listOfValidator));
			});
		}

		private void RunAsyncSharedPerCall(IValidation[] validation, HashSet<IValidation> errorJob,
			AsyncRunState validation2Key)
		{
			RunAsyncTask(() => ObManage(validation, this), validation2Key, validations =>
				{
					HandleErrors(errorJob, validations, validation);
				},
				validation.Select(f => f.GetHashCode().ToString()).Aggregate((e, f) => e + f));
		}

		private void RunSync(IValidation[] validation, HashSet<IValidation> errorJob)
		{
			HandleErrors(errorJob, ObManage(validation, this), validation);
		}

		private void RunSyncToDispatcher(IValidation[] validation, HashSet<IValidation> errorJob)
		{
			BeginThreadSaveAction(() => { RunSync(validation, errorJob); });
		}

		private void RunAuto(IValidation[] validation, HashSet<IValidation> errorJob, AsyncRunState validation2Key)
		{
			if (Thread.CurrentThread == Dispatcher.Thread)
			{
				RunAsyncSharedPerCall(validation, errorJob, validation2Key);
			}
			else
			{
				RunSync(validation, errorJob);
			}
		}

		private void HandleErrorEnumeration(IEnumerable<IValidation> elements, HashSet<IValidation> errorJob, AsyncState asyncState, AsyncRunState asyncRunState)
		{
			switch (asyncState)
			{
				case AsyncState.AsyncSharedPerCall:
					{
						RunAsyncSharedPerCall(elements.ToArray(), errorJob, asyncRunState);
						break;
					}
				case AsyncState.Sync:
					{
						RunSync(elements.ToArray(), errorJob);
						break;
					}
				case AsyncState.SyncToDispatcher:
					{
						RunSyncToDispatcher(elements.ToArray(), errorJob);
						break;
					}
				case AsyncState.NoPreference:
					{
						RunAuto(elements.ToArray(), errorJob, asyncRunState);
						break;
					}
				case AsyncState.Async:
					{
						foreach (var runIndipendendAsync in elements)
						{
							RunAsyncSharedPerCall(new IValidation[] { runIndipendendAsync }, errorJob, asyncRunState);
						}
						break;
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ValidateAsync(string propertyName)
		{
			if (!ErrorMapper.TryGetValue(propertyName, out HashSet<IValidation> errorJob))
			{
				return;
			}
			errorJob.Clear();

			var errors = ProduceErrors(propertyName).ToList();
			lock (_lockRoot)
			{
				_workerCount += errors.Count;
			}
			BeginThreadSaveAction(() => SendPropertyChanged(() => IsValidating));

			var nonAsync = errors.Where(f => !(f is IAsyncValidation)).ToArray();
			if (nonAsync.Any())
			{
				HandleErrorEnumeration(nonAsync, errorJob, AsyncValidationOption.AsyncState, AsyncValidationOption.RunState);
			}

			foreach (var validation in errors.Where(f => f is IAsyncValidation).Cast<IAsyncValidation>().GroupBy(f => f.AsyncState))
			{
				foreach (var validation2 in validation.GroupBy(f => f.RunState))
				{
					HandleErrorEnumeration(validation2, errorJob, validation.Key, validation2.Key);
				}
			}
		}
	}
}