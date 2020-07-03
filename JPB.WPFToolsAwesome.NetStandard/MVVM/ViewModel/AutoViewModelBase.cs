//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Threading;

//namespace JPB.WPFBase.MVVM.ViewModel
//{
//	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
//	sealed class SendPropertyChangeAttribute : Attribute
//	{
//		// See the attribute guidelines at 
//		//  http://go.microsoft.com/fwlink/?LinkId=85236
//		public SendPropertyChangeAttribute()
//		{
			
//		}
//	}

//	public class AutoViewModelBase : ViewModelBase
//	{
//		public AutoViewModelBase()
//		{
			
//		}

//		public AutoViewModelBase(Dispatcher dispatcher) : base(dispatcher)
//		{
			
//		}

//		private static MethodInfo _wrapper;

//		private static void AttachPropertyChangeHandler(object instance)
//		{
//			_wrapper = _wrapper ?? typeof(AutoViewModelBase).GetMethod(nameof(SendPropertyChangedWrapper));

//			var instType = instance.GetType();
//			foreach (var propertyInfo in instType.GetProperties().Where(f => f.GetCustomAttribute<SendPropertyChangeAttribute>() != null))
//			{
//				var propertyInfoSetMethod = propertyInfo.SetMethod;
//				RuntimeHelpers.PrepareMethod(propertyInfoSetMethod.MethodHandle);
//			}
//		}

//		private static MethodInfo CreateChangedWrapper()
//		{
//			MethodBuilder
//		}

//		private static void SendPropertyChangedWrapper(object instance, PropertyInfo property, MethodInfo original)
//		{

//		}
//	}
//}
