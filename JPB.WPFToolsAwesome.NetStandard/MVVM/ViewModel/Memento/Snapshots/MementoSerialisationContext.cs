using System;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel.Memento.Snapshots
{
	public sealed class MementoSerialisationContext : IDisposable
	{
		public MementoSerialisationContext()
		{
			HandleCounter++;
			_serialisationContext = _serialisationContext ?? this;
		}

		public static int HandleCounter { get; private set; }

		internal static object If(object set, object notSet)
		{
			return SerialisationContext == null ? notSet : set;
		}

		[ThreadStatic]
		private static MementoSerialisationContext _serialisationContext;

		public static MementoSerialisationContext SerialisationContext
		{
			get { return _serialisationContext; }
		}

		public void Dispose()
		{
			HandleCounter--;
			if (HandleCounter == 0)
			{
				_serialisationContext = null;
			}
			else if (HandleCounter < 0)
			{
				throw new InvalidOperationException("You cannot dispose the MementoSerialisationContext more then you created it. You called Dispose too often!.");
			}
		}
	}
}