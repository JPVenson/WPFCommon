using System;
using System.Windows.Input;
using System.Windows.Threading;
using JPB.WPFToolsAwesome.MVVM.ViewModel;

namespace JPB.WPFToolsAwesome.MVVM.DelegateCommand
{
	/// <summary>
	///		This Command will wrap an ICommand and extends it with some common UI related properties
	/// </summary>
	public class UiDelegateCommand : ViewModelBase, ICommand
	{
		/// <inheritdoc />
		public UiDelegateCommand(ICommand command)
		{
			Command = command;
		}

		/// <inheritdoc />
		public UiDelegateCommand(Dispatcher dispatcher, ICommand command) : base(dispatcher)
		{
			Command = command;
		}

		private object _caption;
		private object _content;
		private object _tag;

		/// <summary>
		///		A General used Tag
		/// </summary>
		public object Tag
		{
			get { return _tag; }
			set
			{
				SetProperty(ref _tag, value);
			}
		}


		/// <summary>
		///		The Content of this Command
		/// </summary>
		public object Content
		{
			get { return _content; }
			set
			{
				SetProperty(ref _content, value);
			}
		}

		/// <summary>
		///		The Caption of this Command
		/// </summary>
		public object Caption
		{
			get { return _caption; }
			set
			{
				SetProperty(ref _caption, value);
			}
		}

		/// <summary>
		///		The wrapped command
		/// </summary>
		public ICommand Command { get; private set; }

		/// <inheritdoc />
		public bool CanExecute(object parameter)
		{
			return Command.CanExecute(parameter);
		}

		/// <inheritdoc />
		public void Execute(object parameter)
		{
			Command.Execute(parameter);
		}

		/// <inheritdoc />
		public event EventHandler CanExecuteChanged
		{
			add { Command.CanExecuteChanged += value; }
			remove { Command.CanExecuteChanged -= value; }
		}
	}
}
