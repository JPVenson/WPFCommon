using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.WPFBase.MVVM.ViewModel.Memento;

namespace JPB.WpfBase.Tests.MVVM.ViewModel.Memento.Models
{
	public class MementoViewModel : MementoViewModelBase
	{

	}

	public class MementoFullViewModel : MementoViewModelBase
	{
		private string _text;

		public string Text
		{
			get { return _text; }
			set
			{
				SendPropertyChanging(() => Text);
				_text = value;
				SendPropertyChanged(() => Text);
			}
		}
	}

	public class MementoFullWithDateTImeViewModel : MementoFullViewModel
	{
		protected override IMementoDataStamp CreateDataStemp()
		{
			return new MementoDataStampProxy(f => DateTime.Now, new MementoDataStamp());
		}
	}
}
