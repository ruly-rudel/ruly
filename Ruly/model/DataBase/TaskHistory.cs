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
using SQLite;

namespace Ruly.model
{
	public class TaskHistory : System.Object
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

		public DateTime Begin {
			get;
			set;
		}

		public DateTime End {
			get;
			set;
		}

		public TaskHistory Clone()
		{
			return MemberwiseClone () as TaskHistory;
		}
	}
}

