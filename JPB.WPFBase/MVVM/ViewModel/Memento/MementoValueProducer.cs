#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JPB.WPFBase.MVVM.ViewModel.Memento.Attributes;
using JPB.WPFBase.MVVM.ViewModel.Memento.Snapshots;

#endregion

namespace JPB.WPFBase.MVVM.ViewModel.Memento
{
	public class MementoValueProducer
	{
		internal ConcurrentStack<IMementoDataStamp> MementoData { get; set; }
		private readonly MementoOptions _mementoOptions;
		private int _currentAge;
		private bool _resolveFailed;

		private PropertyInfo _propertyInfo;

		/// <summary>
		///		If set no moments are collected for this Property
		/// </summary>
		public bool Ignore { get; set; }

		internal MementoValueProducer(string propertyName, MementoOptions mementoOptions)
		{
			_mementoOptions = mementoOptions;
			PropertyName = propertyName;
			_propertyInfo = null;
			MementoData = new ConcurrentStack<IMementoDataStamp>();
			LockRoot = new object();
		}

		/// <summary>
		///		Returns the Current age of the Property
		/// </summary>
		public int CurrentAge => _currentAge;

		/// <summary>
		///		The name of the Property to watch
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		///		Returns a ReadOnly copy of the Current MementoData
		/// </summary>
		public IEnumerable<IMementoDataStamp> MementoDataStamps => MementoData.ToArray();

		public object LockRoot { get; private set; }

		internal MementoPropertySnaptshot CreateSnapshot()
		{
			var dataSnapshot = MementoDataStamps.ToArray();
			return new MementoPropertySnaptshot
			{
				PropertyName = PropertyName,
				PropertyType = _propertyInfo.PropertyType,
				MementoData = new Stack<IMementoDataStamp>(dataSnapshot)
			};
		}

		private bool ResolvePropertyInfoIfUnset(object viewModel)
		{
			if (_resolveFailed)
			{
				return false;
			}

			if (_propertyInfo == null)
			{
				if (_mementoOptions.ResolveProperty != null)
				{
					_propertyInfo = _mementoOptions.ResolveProperty(viewModel, PropertyName);
				}
				else
				{
					_propertyInfo = viewModel.GetType().GetProperty(PropertyName);
				}
			}

			if (_propertyInfo == null)
			{
				_resolveFailed = true;
				Trace.TraceWarning(
				$"JPB.MementoPattern: There is no Property '{PropertyName}' on ViewModel '{viewModel.GetType().FullName}' ");
				return false;
			}

			if (_propertyInfo.GetCustomAttribute<IgnoreMementoAttribute>() != null)
			{
				Ignore = true;
			}

			return true;
		}

		internal object GetValue(object viewModel)
		{
			if (!ResolvePropertyInfoIfUnset(viewModel))
			{
				return null;
			}

			var data = _propertyInfo.GetValue(viewModel);

			if (data != null && _mementoOptions.TryCloneData && data is ICloneable)
			{
				data = (data as ICloneable).Clone();
			}

			return data;
		}

		internal void SetValue(object viewModel, object value)
		{
			if (!ResolvePropertyInfoIfUnset(viewModel))
			{
				return;
			}

			try
			{
				MementoViewModelBase.DoNotSetMoment = true;
				_propertyInfo.SetValue(viewModel, value);
			}
			finally
			{
				MementoViewModelBase.DoNotSetMoment = false;
			}
		}

		internal bool TryAdd(MementoViewModelBase mementoViewModelBase, IMementoDataStamp dataStemp)
		{
			if (Ignore)
			{
				return false;
			}

			var moment = GetValue(mementoViewModelBase);
			if (!dataStemp.CanSetData(moment))
			{
				return false;
			}

			var currentMoment = MementoData.FirstOrDefault();

			if (currentMoment != null && currentMoment.GetData() == moment)
			{
				return false;
			}

			dataStemp.CaptureData(moment);
			lock (LockRoot)
			{
				while (_currentAge < MementoData.Count)
				{
					IMementoDataStamp outdatedStamp;
					if (MementoData.TryPop(out outdatedStamp))
					{
						outdatedStamp.Forget();
					}
				}

				MementoData.Push(dataStemp);
				_currentAge++;
			}

			return true;
		}

		/// <summary>
		///		Forgets every Moment
		/// </summary>
		public void Forget()
		{
			lock (LockRoot)
			{
				while (!MementoData.IsEmpty)
				{
					IMementoDataStamp outdatedStamp;
					if (MementoData.TryPop(out outdatedStamp))
					{
						outdatedStamp.Forget();
					}

					_currentAge--;
				}
			}
		}
		///  <summary>
		/// 		Goes forth or back in History. If <para>ages</para> is negative it goes back. if positive it goes forth
		///  </summary>
		/// <param name="viewModel"></param>
		/// <param name="ages"></param>
		///  <returns></returns>
		public void GoInHistory(MementoViewModelBase viewModel, int ages)
		{
			lock (LockRoot)
			{
				if (!CanGoInHistory(ages))
				{
					return;
				}

				var toAge = _currentAge + ages;
				_currentAge = toAge;
				var ageValueHolder = MementoDataStamps.ElementAt(_currentAge - 1);

				if (!ageValueHolder.CanGetData())
				{
					return;
				}

				var ageValue = ageValueHolder.GetData();
				SetValue(viewModel, ageValue);
			}
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="ages"></param>
		/// <returns></returns>
		public bool CanGoInHistory(int ages)
		{
			var toAge = _currentAge + ages;
			return toAge > 0 && toAge < MementoData.Count;
		}
	}
}