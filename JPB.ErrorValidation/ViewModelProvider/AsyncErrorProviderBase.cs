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
			AsyncValidationOption = new AsyncValidationOption();
			AsyncValidationOption.AsyncState = AsyncState.AsyncSharedPerCall;
			AsyncValidationOption.RunState = AsyncRunState.CurrentPlusOne;
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
			HashSet<IValidation> errorJob = null;
			ErrorMapper.TryGetValue(propertyName, out errorJob);
			if (errorJob != null)
				return errorJob.Select(f => ValidationToUiError == null ? f : ValidationToUiError(f));
			return null;
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
			var handler = ErrorsChanged;
			if (handler != null) handler(this, e);
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

		private void HandleErrors(HashSet<IValidation> source, IValidation[] newErrors, IEnumerable<IValidation> processed)
		{
			foreach (var validation in newErrors)
			{
				source.Add(validation);
			}

			source.RemoveWhere(f => processed.Contains(f) && !newErrors.Contains(f));
			//var oldErrors = source.Intersect(newErrors);
		}

		private void RunAsyncSharedPerCall(IEnumerable<IValidation> validation, HashSet<IValidation> errorJob,
			AsyncRunState validation2Key)
		{
			var enumerable = validation as IValidation[] ?? validation.ToArray();
			RunAsyncTask(() => ObManage(enumerable, this), validation2Key, validations =>
				{
					for (int i = 0; i < enumerable.Count(); i++)
					{
						Interlocked.Decrement(ref _workerCount);
					}
					HandleErrors(errorJob, validations, enumerable);
					//foreach (var validation1 in validations)
					//{
					//	errorJob.Add(validation1);
					//}
					BeginThreadSaveAction(() =>
					{
						SendPropertyChanged(() => IsValidating);
						foreach (var listOfValidator in validations.SelectMany(f => f.ErrorIndicator).Distinct())
							OnErrorsChanged(new DataErrorsChangedEventArgs(listOfValidator));
					});
				},
				enumerable.Select(f => f.GetHashCode().ToString()).Aggregate((e, f) => e + f));
		}

		private void RunSync(IEnumerable<IValidation> validation, HashSet<IValidation> errorJob, AsyncRunState validation2Key)
		{
			var errorsForField = validation as IValidation[] ?? validation.ToArray();

			HandleErrors(errorJob, ObManage(errorsForField, this), errorsForField);
			//foreach (var validation1 in ObManage(errorsForField, this))
			//	errorJob.Add(validation1);
			for (var i = 0; i < errorsForField.Count(); i++)
				Interlocked.Decrement(ref _workerCount);
			BeginThreadSaveAction(() =>
			{
				SendPropertyChanged(() => IsValidating);
				foreach (var listOfValidator in errorsForField.SelectMany(f => f.ErrorIndicator).Distinct())
					OnErrorsChanged(new DataErrorsChangedEventArgs(listOfValidator));
			});
		}

		private void RunSyncToDispatcher(IEnumerable<IValidation> validation, HashSet<IValidation> errorJob,
			AsyncRunState validation2Key)
		{
			BeginThreadSaveAction(() => { RunSync(validation, errorJob, validation2Key); });
		}

		private void RunAuto(IEnumerable<IValidation> validation, HashSet<IValidation> errorJob, AsyncRunState validation2Key)
		{
			if (Thread.CurrentThread == Dispatcher.Thread)
				RunAsyncSharedPerCall(validation, errorJob, validation2Key);
			else
				RunSync(validation, errorJob, validation2Key);
		}

		private void ValidateAsync(string propertyName)
		{
			HashSet<IValidation> errorJob = null;
			if (!ErrorMapper.TryGetValue(propertyName, out errorJob))
				return;
			errorJob.Clear();

			var errors = ProduceErrors(propertyName).ToList();

			Interlocked.Add(ref _workerCount, errors.Count);
			BeginThreadSaveAction(() => SendPropertyChanged(() => IsValidating));

			var nonAsync = errors.Where(f => !(f is IAsyncValidation)).ToArray();
			if (nonAsync.Any())
				RunAsyncSharedPerCall(nonAsync, errorJob, AsyncValidationOption.RunState);

			foreach (var validation in errors.Where(f => f is IAsyncValidation).Cast<IAsyncValidation>().GroupBy(f => f.AsyncState))
			{
				foreach (var validation2 in validation.GroupBy(f => f.RunState))
				{
					switch (validation.Key)
					{
						case AsyncState.AsyncSharedPerCall:
							{
								RunAsyncSharedPerCall(validation2, errorJob, validation2.Key);
								break;
							}
						case AsyncState.Sync:
							{
								RunSync(validation2, errorJob, validation2.Key);
								break;
							}
						case AsyncState.SyncToDispatcher:
							{
								RunSyncToDispatcher(validation2, errorJob, validation2.Key);
								break;
							}
						case AsyncState.NoPreference:
							{
								RunAuto(validation2, errorJob, validation2.Key);
								break;
							}
						case AsyncState.Async:
							{
								foreach (var runIndipendendAsync in validation2)
								{
									RunAsyncSharedPerCall(new IValidation[]{ runIndipendendAsync }, errorJob, validation2.Key);
								}
								break;
							}
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
		}
	}
}