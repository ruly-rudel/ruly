using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ruly.viewmodel
{
	public class ShellViewModelData : INotifyPropertyChanged
	{
		private string todayDate;
		public string TodayDate {
			get {
				return todayDate;
			}
			set {
				todayDate = value;
				NotifyPropertyChanged ();
			}
		}

		private string todayDay;
		public string TodayDay {
			get {
				return todayDay;
			}
			set {
				todayDay = value;
				NotifyPropertyChanged ();
			}
		}

		private string todayTime;
		public string TodayTime {
			get {
				return todayTime;
			}
			set {
				todayTime = value;
				NotifyPropertyChanged ();
			}
		}

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}

