using System.Threading.Tasks;
using System.Windows.Threading;

namespace JPB.WPFToolsAwesome.MVVM.ViewModel
{
	/// <summary>
	///		The wrapper for <seealso cref="System.Windows.Threading.DispatcherOperation"/>
	/// </summary>
	public class DispatcherOperationLite
	{
		private readonly DispatcherOperation _operation;
		private readonly Dispatcher _dispatcher;
		private readonly DispatcherPriority _priority;
		private readonly DispatcherOperationStatus _status;

		/// <summary>
		/// Initializes a new instance of the <see cref="DispatcherOperationLite"/> class.
		/// </summary>
		/// <param name="operation">The operation.</param>
		public DispatcherOperationLite(DispatcherOperation operation)
		{
			_operation = operation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DispatcherOperationLite"/> class.
		/// </summary>
		/// <param name="dispatcher">The dispatcher.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="status">The status.</param>
		public DispatcherOperationLite(Dispatcher dispatcher, DispatcherPriority priority, DispatcherOperationStatus status)
		{
			_dispatcher = dispatcher;
			_priority = priority;
			_status = status;
		}

		/// <summary>Gets the <see cref="T:System.Windows.Threading.Dispatcher" /> that the operation was posted to. </summary>
		/// <returns>The dispatcher.</returns>
		public Dispatcher Dispatcher
		{
			get { return _operation?.Dispatcher ?? _dispatcher; }
		}

		/// <summary>Gets or sets the priority of the operation in the <see cref="T:System.Windows.Threading.Dispatcher" /> queue. </summary>
		/// <returns>The priority of the delegate on the queue.</returns>
		public DispatcherPriority Priority
		{
			get { return _operation?.Priority ?? _priority; }
		}

		/// <summary>Gets the current status of the operation..</summary>
		/// <returns>The status of the operation.</returns>
		public DispatcherOperationStatus Status
		{
			get { return _operation?.Status ?? _status; }
		}

		/// <summary>
		/// Gets the task.
		/// </summary>
		/// <returns></returns>
		public Task GetTask()
		{
			return _operation.Task ?? Task.CompletedTask;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Abort()
		{
			return _operation?.Abort() ?? false;
		}
	}
}