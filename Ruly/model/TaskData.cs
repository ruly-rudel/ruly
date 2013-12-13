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
using SQLite;

namespace Ruly.model
{
	public class TaskData : System.Object, INotifyPropertyChanged
	{
		[PrimaryKey, AutoIncrement]
		public int Id {
			get;
			set;
		}

		private string title;
		public string Title {
			get {
				return title;
			}
			set {
				title = value;
				NotifyPropertyChanged ();
			}
		}

		private int state;
		public int State {
			get {
				return state;
			}
			set {
				state = value;
				NotifyPropertyChanged ();
			}
		}

		private int place;
		public int Place {
			get {
				return place;
			}
			set {
				place = value;
				NotifyPropertyChanged ();
			}
		}

		private int parent;
		[Indexed]
		public int Parent {
			get {
				return parent;
			}
			set {
				parent = value;
				NotifyPropertyChanged ();
			}
		}

		private DateTime createDateTime;
		public DateTime CreateDateTime {
			get {
				return createDateTime;
			}
			set {
				createDateTime = value;
				NotifyPropertyChanged ();
			}
		}

		private DateTime lastModifiedDateTime;
		public DateTime LastModifiedDateTime {
			get {
				return lastModifiedDateTime;
			}
			set {
				lastModifiedDateTime = value;
				NotifyPropertyChanged ();
			}
		}

		private DateTime dueDateTime;
		public DateTime DueDateTime {
			get {
				return dueDateTime;
			}
			set {
				dueDateTime = value;
				NotifyPropertyChanged ();
			}
		}

		private string note;
		public String Note {
			get {
				return note;
			}
			set {
				note = value;
				NotifyPropertyChanged ();
			}
		}

		public TaskData Clone()
		{
			return MemberwiseClone () as TaskData;
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

