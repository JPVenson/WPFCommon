using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using JetBrains.Annotations;

namespace JPB.WPFToolsAwesome.Behaviors.Eval.Trigger
{
	/// <summary>
	///     The base Implementation for an Trigger. If there are any Dependency properties bound,
	///     they should Ether call RegisterDependencyProperty or at least use the <see cref="PropertyChangedCallback" />.
	///     <para></para>
	///     <remarks>
	///         If custom implementation is made take care to invoke the <see cref="OnPropertyChanged" /> and
	///         <see cref="PropertyChanging" /> whenever any of your bindings have changed to reevaluate the whole chain
	///     </remarks>
	/// </summary>
	public abstract class TriggerStepBase : FrameworkElement, ITriggerStep
	{
		public abstract bool Evaluate(object dataContext, DependencyObject dependencyObject);

		public virtual void SetDataContext(object dataContext)
		{
			DataContext = dataContext;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///		Registers the Triggers <see cref="INotifyPropertyChanged.PropertyChanged"/> event to the <see cref="DependencyProperty"/>s <see cref="PropertyMetadata.PropertyChangedCallback"/> 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="propertyType"></param>
		/// <param name="ownerType"></param>
		/// <param name="typeMetadata"></param>
		/// <returns></returns>
		public static DependencyProperty Register(
			string name,
			Type propertyType,
			Type ownerType,
			PropertyMetadata typeMetadata)
		{
			var originalChanged = typeMetadata.PropertyChangedCallback;
			var originalCoerce = typeMetadata.CoerceValueCallback;

			typeMetadata.PropertyChangedCallback = (o, args) =>
			{
				originalChanged?.Invoke(o, args);
				PropertyChangedCallback(o, args);
			};

			return DependencyProperty.Register(name, propertyType, ownerType, typeMetadata);
		}

		public static IEnumerable<DependencyProperty> GetDependencyProperties(Type obj, bool inherted = false)
		{
			var flags = BindingFlags.Static |
			            BindingFlags.Public;
			if (inherted)
			{
				flags |= BindingFlags.FlattenHierarchy;
			}

			return obj.GetFields(flags)
				.Where(f => f.FieldType == typeof(DependencyProperty))
				.Select(f => f.GetValue(null) as DependencyProperty)
				.ToArray();
		}

		protected static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (Equals(e.OldValue, e.NewValue))
			{
				return;
			}

			if (d is TriggerStepBase bas)
			{
				bas.OnPropertyChanging(e.Property, e.OldValue, e.NewValue);
				bas.OnPropertyChanged(e.Property.Name);
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event Action<DependencyProperty, object, object> PropertyChanging;

		protected virtual void OnPropertyChanging(DependencyProperty property, object oldValue, object newValue)
		{
			PropertyChanging?.Invoke(property, oldValue, newValue);
		}
	}
}