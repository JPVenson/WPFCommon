using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace JPB.WPFBase.MVVM.ViewModel
{
	/// <summary>
	///     Defines alternate names for raising the INotifyPropertyChanged event.
	///		All Methods call SendPropertyChanged. Only for Migration.
	/// </summary>
	public static class ViewModelBaseINPCCommonNames
	{
		[PublicAPI]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RaisePropertyChanged(this ViewModelBase viewModel,
			[CallerMemberName] string propertyName = null)
		{
			viewModel.SendPropertyChanged(propertyName);
		}

		[PublicAPI]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RaisePropertyChanged<TProperty>(this ViewModelBase viewModel,
			Expression<Func<TProperty>> property)
		{
			viewModel.SendPropertyChanged(property);
		}

		[PublicAPI]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void OnPropertyChanged(this ViewModelBase viewModel,
			[CallerMemberName] string propertyName = null)
		{
			viewModel.SendPropertyChanged(propertyName);
		}

		[PublicAPI]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void OnPropertyChanged<TProperty>(this ViewModelBase viewModel,
			Expression<Func<TProperty>> property)
		{
			viewModel.SendPropertyChanged(property);
		}

		[PublicAPI]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FirePropertyChanged(this ViewModelBase viewModel,
			[CallerMemberName] string propertyName = null)
		{
			viewModel.SendPropertyChanged(propertyName);
		}

		[PublicAPI]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FirePropertyChanged<TProperty>(this ViewModelBase viewModel,
			Expression<Func<TProperty>> property)
		{
			viewModel.SendPropertyChanged(property);
		}
	}
}