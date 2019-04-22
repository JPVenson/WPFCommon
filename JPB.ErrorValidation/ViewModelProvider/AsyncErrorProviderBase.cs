using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using JPB.ErrorValidation.ValidationTyps;
using JPB.ErrorValidation.ViewModelProvider.Base;
using JPB.Tasking.TaskManagement.Threading;

namespace JPB.ErrorValidation.ViewModelProvider
{
	/// <summary>
	///   Provides the INotifyDataErrorInfo Interface
	/// </summary>
	public abstract class AsyncErrorProviderBase :
		ErrorProviderBase, INotifyDataErrorInfo
	{
		private readonly SingelSerialTaskFactory _errorFactory = new SingelSerialTaskFactory(false, 2);
		private readonly object _lockRoot = new object();
		private int _workerCount;

		protected AsyncErrorProviderBase(Dispatcher dispatcher, IErrorCollectionBase errors) : base(dispatcher, errors)
		{
			ErrorMapper = new ConcurrentDictionary<string, HashSet<IValidation>>();

			PropertyChanged += AsyncErrorProviderBase_PropertyChanged;
			Load();
			AsyncValidationOption = new AsyncValidationOption
			{
				AsyncState = AsyncState.AsyncSharedPerCall,
				RunState = AsyncRunState.CurrentPlusOne
			};
		}

		public AsyncErrorProviderBase(IErrorCollectionBase errors) 
			: this(null, errors)
		{
		}

		/// <summary>
		///   If an Rendering is Requested, the ui should use this
		/// </summary>
		protected Func<IValidation, object> ValidationToUiError { get; set; }

		/// <summary>
		///   The Default execution if the Item is no IAsyncValidation
		/// </summary>
		protected virtual IAsyncValidationOption AsyncValidationOption { get; }

		/// <summary>
		///   Is currently Validating
		/// </summary>
		public bool IsValidating
		{
			get { return _workerCount > 0; }
		}

		/// <summary>
		///   Gets all Error lists for all columns
		/// </summary>
		public ConcurrentDictionary<string, HashSet<IValidation>> ErrorMapper { get; }

		/// <inheritdoc />
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		/// <inheritdoc />
		public IEnumerable GetErrors(string propertyName)
		{
			if (propertyName == null)
			{
				propertyName = string.Empty;
			}

			ErrorMapper.TryGetValue(propertyName, out var errorJob);
			return errorJob?.Select(f => ValidationToUiError == null ? f : ValidationToUiError(f));
		}

		/// <inheritdoc />
		public bool HasErrors
		{
			get { return HasError; }
		}

		private void Load()
		{
			UserErrors.CollectionChanged += UserErrorsCollectionChanged;

			foreach (var validation in UserErrors.SelectMany(f => f.ErrorIndicator).Distinct())
			{
				ErrorMapper.GetOrAdd(validation, new HashSet<IValidation>());
			}
		}

		private void UserErrorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (IValidation newValidation in e.NewItems)
				{
					foreach (var indicator in newValidation.ErrorIndicator)
					{
						var errHashset = ErrorMapper.GetOrAdd(indicator, new HashSet<IValidation>());
						errHashset.Add(newValidation);
					}
				}
			}

			if (e.OldItems != null)
			{
				foreach (IValidation newValidation in e.OldItems)
				{
					foreach (var indicator in newValidation.ErrorIndicator)
					{
						var errHashset = ErrorMapper.GetOrAdd(indicator, new HashSet<IValidation>());
						errHashset.Remove(newValidation);
						if (!errHashset.Any())
						{
							ErrorMapper.TryRemove(indicator, out errHashset);
						}
					}
				}
			}
		}

		/// <inheritdoc />
		public override void ForceRefresh()
		{
			if (ErrorMapper != null)
			{
				foreach (var errorMap in ErrorMapper)
				{
					ValidateAsync(errorMap.Key);
				}
			}
		}

		/// <summary>
		///		Invokes the Validation for a single Property
		/// </summary>
		/// <typeparam name="TProp">The type of the property.</typeparam>
		/// <param name="property">The property.</param>
		public void ScheduleErrorUpdate<TProp>(Expression<Func<TProp>> property)
		{
			// ReSharper disable once ExplicitCallerInfoArgument
			ScheduleErrorUpdate(GetPropertyName(property));
		}

		/// <summary>
		///   Schedules a new Update for the Property
		/// </summary>
		/// <param name="propertyName"></param>
		public void ScheduleErrorUpdate([CallerMemberName] string propertyName = null)
		{
			if (propertyName != null && ErrorMapper.ContainsKey(propertyName))
			{
				ValidateAsync(propertyName);
			}
		}

		private void AsyncErrorProviderBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// ReSharper disable once ExplicitCallerInfoArgument
			ScheduleErrorUpdate(e.PropertyName);
		}

		/// <summary>
		/// Raises the <see cref="E:ErrorsChanged" /> event.
		/// </summary>
		/// <param name="e">The <see cref="DataErrorsChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
		{
			ErrorsChanged?.Invoke(this, e);
		}

		private void RunAsyncTask(Func<IValidation[]> gerValidations,
			AsyncRunState validation2Key,
			Action<IValidation[]> then,
			string key)
		{
			switch (validation2Key)
			{
				case AsyncRunState.NoPreference:
					if (_errorFactory.ConcurrentQueue.Count >= Environment.ProcessorCount)
					{
						_errorFactory.TryAdd(() =>
						{
							var exec = gerValidations();
							BeginThreadSaveAction(() => { then(exec); });
						}, key);
					}
					else
					{
						SimpleWork(gerValidations, then);
					}

					break;
				case AsyncRunState.CurrentPlusOne:
					_errorFactory.TryAdd(() =>
					{
						var exec = gerValidations();
						BeginThreadSaveAction(() => { then(exec); });
					}, key);
					break;
				case AsyncRunState.OnlyOnePerTime:
					_errorFactory.TryAdd(() =>
					{
						var exec = gerValidations();
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
					foreach (var run in elements)
					{
						RunAsyncSharedPerCall(new[] {run}, asyncRunState);
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

			public List<HashSet<IValidation>> AffectedValidators { get; set; }

			public void AddErrorToField(IValidation error)
			{
				AffectedValidators = new List<HashSet<IValidation>>();
				foreach (var indicator in error.ErrorIndicator)
				{
					var field = ErrorMapper.GetOrAdd(indicator, new HashSet<IValidation>());
					field.Add(error);
					AffectedValidators.Add(field);
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

	public class AsyncErrorProviderBase<T> : AsyncErrorProviderBase where T : IErrorCollectionBase, new()
	{
		public AsyncErrorProviderBase(Dispatcher dispatcher) : base(dispatcher, new T())
		{
		}

		public AsyncErrorProviderBase() : this(null)
		{
		}
	}
}