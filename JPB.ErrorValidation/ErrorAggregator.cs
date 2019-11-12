using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.ErrorValidation
{
	/// <summary>
	///     Aggregates errors from several ViewModels
	/// </summary>
	public class ErrorAggregator : ViewModelBase, INotifyDataErrorInfo
	{
		public ErrorAggregator()
		{
			ErrorBindingRoot = new object();
			Provider = new HashSet<WeakReference<INotifyDataErrorInfo>>();
			Validations = new ConcurrentDictionary<INotifyDataErrorInfo, IList<object>>();
		}

		public ErrorAggregator(Dispatcher dispatcher) : base(dispatcher)
		{
			ErrorBindingRoot = new object();
			Provider = new HashSet<WeakReference<INotifyDataErrorInfo>>();
			Validations = new ConcurrentDictionary<INotifyDataErrorInfo, IList<object>>();
		}

		/// <summary>
		///     Bind this property and display the errors with the standard WPF error control
		/// </summary>
		public object ErrorBindingRoot { get; }

		public ConcurrentDictionary<INotifyDataErrorInfo, IList<object>> Validations { get; private set; }

		private HashSet<WeakReference<INotifyDataErrorInfo>> Provider { get; set; }

		public IEnumerable<INotifyDataErrorInfo> GetProvider()
		{
			return Provider.Select(f =>
			{
				f.TryGetTarget(out var target);
				return target;
			}).Where(e => e != null);
		}

		public IEnumerable GetErrors(string propertyName)
		{
			if (propertyName == nameof(ErrorBindingRoot))
			{
				return Validations;
			}

			return Enumerable.Empty<object>();
		}

		public bool HasErrors
		{
			get { return Validations.Any(f => f.Value.Any()); }
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		/// <summary>
		///     Adds the given provider and then enumerates all properties for error provider
		/// </summary>
		/// <param name="errorProvider"></param>
		public void AddAll(INotifyDataErrorInfo errorProvider)
		{
			WeakEventManager<INotifyDataErrorInfo, DataErrorsChangedEventArgs>
				.AddHandler(errorProvider, nameof(ErrorsChanged), HandleErrorChanged);

			var stack = new Stack<INotifyDataErrorInfo>();
			stack.Push(errorProvider);
			while (stack.Any())
			{
				var item = stack.Pop();
				Add(item);
				var propertyInfos = item.GetType().GetProperties(BindingFlags.Public & BindingFlags.Instance)
					.Where(f => typeof(INotifyDataErrorInfo).IsAssignableFrom(f.PropertyType));
				foreach (var propertyInfo in propertyInfos)
				{
					if (propertyInfo.GetValue(item) is INotifyDataErrorInfo notifyDataErrorInfo)
					{
						stack.Push(notifyDataErrorInfo);
					}
				}
			}
		}

		/// <summary>
		///     Adds a single provider of Errors
		/// </summary>
		/// <param name="errorProvider"></param>
		public void Add(INotifyDataErrorInfo errorProvider)
		{
			WeakEventManager<INotifyDataErrorInfo, DataErrorsChangedEventArgs>
				.AddHandler(errorProvider, nameof(ErrorsChanged), HandleErrorChanged);
			Provider.Add(new WeakReference<INotifyDataErrorInfo>(errorProvider));
		}

		/// <summary>
		///     Removes a single provider of Errors
		/// </summary>
		/// <param name="errorProvider"></param>
		public bool Remove(INotifyDataErrorInfo errorProvider)
		{
			if (Validations.ContainsKey(errorProvider) && !Validations.TryRemove(errorProvider, out _))
			{
				return false;
			}

			Provider.RemoveWhere(f => f.TryGetTarget(out var provider) && provider == errorProvider);

			WeakEventManager<INotifyDataErrorInfo, DataErrorsChangedEventArgs>
				.RemoveHandler(errorProvider, nameof(ErrorsChanged), HandleErrorChanged);
			return true;
		}

		private void HandleErrorChanged(object sender, DataErrorsChangedEventArgs e)
		{
			if (sender is INotifyDataErrorInfo errorInfo)
			{
				var enumerable = errorInfo.GetErrors(string.Empty);
				var errors = Validations.GetOrAdd(errorInfo, info => new List<object>());
				errors.Clear();
				foreach (var error in enumerable)
				{
					errors.Add(error);
				}

				SendPropertyChanged(() => HasErrors);
				OnErrorsChanged(new DataErrorsChangedEventArgs(nameof(ErrorBindingRoot)));
			}
		}

		protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
		{
			ErrorsChanged?.Invoke(this, e);
		}
	}
}