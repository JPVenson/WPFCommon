using JPB.WPFBase.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using JPB.WPFBase.MVVM.DelegateCommand;

namespace JPB.DataInputWindow.ViewModel
{
	public class DataImportViewModel : ViewModelBase
	{
		public DataImportViewModel()
		{
			AcceptValuesCommand = new DelegateCommand(AcceptValuesExecute, CanAcceptValuesExecute);
			AbortCommand = new DelegateCommand(AbortExecute, CanAbortExecute);
			Fields = new ObservableCollection<DataImportFieldViewModelBase>();
		}

		public ObservableCollection<DataImportFieldViewModelBase> Fields { get; set; }

		public IEnumerable<KeyValuePair<string, object>> GetValues()
		{
			return Fields.Select(f => new KeyValuePair<string, object>(f.Key, f.Value));
		}

		private bool _allowAbort;
		private bool _result;

		public bool Result
		{
			get { return _result; }
			set
			{
				SendPropertyChanging(() => Result);
				_result = value;
				SendPropertyChanged(() => Result);
			}
		}

		public bool AllowAbort
		{
			get { return _allowAbort; }
			set
			{
				SendPropertyChanging(() => AllowAbort);
				_allowAbort = value;
				SendPropertyChanged(() => AllowAbort);
			}
		}

		public DelegateCommand AcceptValuesCommand { get; private set; }
		public DelegateCommand AbortCommand { get; private set; }

		private void AbortExecute(object sender)
		{
			Result = false;
		}

		private bool CanAbortExecute(object sender)
		{
			return AllowAbort;
		}

		private void AcceptValuesExecute(object sender)
		{
			Result = true;
		}

		private bool CanAcceptValuesExecute(object sender)
		{
			return Fields.All(e => !e.HasError);
		}
	}
}