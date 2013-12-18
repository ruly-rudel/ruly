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
using System.Collections.ObjectModel;

namespace Ruly.model
{
	public class CombinedTaskHistory : System.Object, INotifyPropertyChanged
	{
		private int id;
		public int Id {
			get {
				return id;
			}
			set {
				id = value;
				NotifyPropertyChanged ();
			}
		}

		public ObservableCollection<int> TaskId {
			get;
			set;
		}
//		private int taskId;
//		public int TaskId {
//			get {
//				return taskId;
//			}
//			set {
//				taskId = value;
//				NotifyPropertyChanged();
//			}
//		}

		private DateTime begin;
		public DateTime Begin {
			get {
				return begin;
			}
			set {
				begin = value;
				NotifyPropertyChanged ();
			}
		}

		private DateTime end;
		public DateTime End {
			get {
				return end;
			}
			set {
				end = value;
				NotifyPropertyChanged ();
			}
		}

		private TimeSpan rest;
		public TimeSpan Rest {
			get {
				return rest;
			}
			set {
				rest = value;
				NotifyPropertyChanged ();
			}
		}

		public CombinedTaskHistory Clone()
		{
			return MemberwiseClone () as CombinedTaskHistory;
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

