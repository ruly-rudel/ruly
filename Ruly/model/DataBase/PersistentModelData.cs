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

namespace Ruly.model
{
	public class PersistentModelData
	{
		public int state { get; set; }	// 0: idle, 1:working, 2;pause
		public TaskHistory current_exec { get; set; }
		public TaskData current_task { get; set; }

		public PersistentModelData()
		{
			state = 0;	// idle
			current_exec = null;
			current_task = null;
		}
	}
}

