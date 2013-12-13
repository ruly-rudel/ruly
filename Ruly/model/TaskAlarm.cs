using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace Ruly.model
{
	public class TaskAlarm : System.Object, INotifyPropertyChanged
	{
		[PrimaryKey, AutoIncrement]
		public int Id {
			get;
			set;
		}

		[Indexed]
		public int TaskId {
			get;
			set;
		}

		private DateTime alarmDateTime;
		public DateTime AlarmDateTime {
			get {
				return alarmDateTime;
			}
			set {
				alarmDateTime = value;
				NotifyPropertyChanged ();
			}
		}

		public TaskAlarm Clone()
		{
			return MemberwiseClone () as TaskAlarm;
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

