#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JPB.WPFToolsAwesome.MVVM.ViewModel.Memento.Attributes;
using JPB.WPFToolsAwesome.MVVM.ViewModel.Memento.Snapshots;

#endregion

namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Memento
{
	public class MementoValueProducer : IMementoValueHolder
	{
		ConcurrentStack<IMementoDataStamp> IMementoValueHolder.MementoData { get; set; }
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
			((IMementoValueHolder) this).MementoData = new ConcurrentStack<IMementoDataStamp>();
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
		public IEnumerable<IMementoDataStamp> MementoDataStamps => ((IMementoValueHolder) this).MementoData.ToArray();
		
		MementoPropertySnaptshot IMementoValueHolder.CreateSnapshot()
		{
			if (_propertyInfo == null)
			{
				return null;
			}

			var dataSnapshot = MementoDataStamps.ToArray();
			return new MementoPropertySnaptshot
			{
				PropertyName = PropertyName,
				PropertyType = _propertyInfo.PropertyType,
				MementoData = new Stack<IMementoDataStamp>(dataSnapshot)
			};
		}

		private bool ResolvePropertyInfoIfUnset(MementoViewModelBase viewModel)
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

			if (_propertyInfo == null || !_propertyInfo.CanRead || !_propertyInfo.CanWrite)
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

		object IMementoValueHolder.GetValue(MementoViewModelBase viewModel)
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

		internal void SetValue(MementoViewModelBase viewModel, object value)
		{
			if (!_propertyInfo.CanWrite || !ResolvePropertyInfoIfUnset(viewModel))
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

		bool IMementoValueHolder.TryAdd(MementoViewModelBase mementoViewModelBase, IMementoDataStamp dataStemp)
		{
			if (Ignore)
			{
				return false;
			}

			var result = false;
			mementoViewModelBase.ThreadSaveAction(() =>
			{
				var moment = ((IMementoValueHolder)this).GetValue(mementoViewModelBase);
				if (!dataStemp.CanSetData(moment))
				{
					return;
				}

				var memntoData = ((IMementoValueHolder)this).MementoData;

				var currentMoment = memntoData.FirstOrDefault();

				if (currentMoment != null && currentMoment.GetData() == moment)
				{
					return;
				}

				dataStemp.CaptureData(moment);
				while (_currentAge < memntoData.Count)
				{
					IMementoDataStamp outdatedStamp;
					if (memntoData.TryPop(out outdatedStamp))
					{
						outdatedStamp.Forget();
					}
				}

				memntoData.Push(dataStemp);
				_currentAge++;

				result = true;
			});

			return result;
		}

		/// <summary>
		///		Forgets every Moment
		/// </summary>
		public void Forget()
		{
			while (!((IMementoValueHolder)this).MementoData.IsEmpty)
			{
				IMementoDataStamp outdatedStamp;
				if (((IMementoValueHolder)this).MementoData.TryPop(out outdatedStamp))
				{
					outdatedStamp.Forget();
				}

				_currentAge--;
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
			viewModel.ThreadSaveAction(() =>
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
			});
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="ages"></param>
		/// <returns></returns>
		public bool CanGoInHistory(int ages)
		{
			var toAge = _currentAge + ages;
			return toAge > 0 && toAge < ((IMementoValueHolder) this).MementoData.Count;
		}
	}
}