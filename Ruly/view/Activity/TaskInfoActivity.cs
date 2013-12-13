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

using Ruly.viewmodel;

namespace Ruly.view
{
	[Activity (Label = "TaskInfoActivity"/*, UiOptions = Android.Content.PM.UiOptions.SplitActionBarWhenNarrow*/)]			
	public class TaskInfoActivity : Activity
	{
		ListView taskInfoList;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.TaskInfo);

			taskInfoList = FindViewById<ListView> (Resource.Id.taskInfoList);
			taskInfoList.Adapter = new TaskInfoAdapter(LayoutInflater, ViewModel.SelectedTask);
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.info_menu, menu);
			return true;
		}

		public override void OnBackPressed ()
		{
			ViewModel.UnselectTask ();
			base.OnBackPressed ();
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case Resource.Id.menu_go:
				ViewModel.StartSelectedTask ();
				ViewModel.UnselectTask ();
				Finish ();
				return true;

			case Resource.Id.menu_edit:
				StartActivity (typeof(TaskEditActivity));
				return true;

			default:
				return false;
			}
		}
	}
}

