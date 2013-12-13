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
using Android.Support.V4.App;
using Android.Support.V4.View;

using Ruly.viewmodel;

namespace Ruly.view
{
	[Activity (Label = "TaskEditActivity")]			
	public class TaskEditActivity : FragmentActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.TaskEditHome);
			var fragment = new TaskEditFragment ();
			SupportFragmentManager.BeginTransaction ().Add (Resource.Id.taskEditHome, fragment).Commit ();
		}
	}
}

