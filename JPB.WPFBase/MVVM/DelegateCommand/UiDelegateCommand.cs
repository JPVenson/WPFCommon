using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.WPFBase.MVVM.DelegateCommand
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
				SendPropertyChanging(() => Tag);
				_tag = value;
				SendPropertyChanged(() => Tag);
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
				SendPropertyChanging(() => Content);
				_content = value;
				SendPropertyChanged(() => Content);
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
				SendPropertyChanging(() => Caption);
				_caption = value;
				SendPropertyChanged(() => Caption);
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
