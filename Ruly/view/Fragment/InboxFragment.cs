using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;

using Ruly.viewmodel;

namespace Ruly.view
{
	public class InboxFragment : Android.Support.V4.App.Fragment
	{
		View rootView;
		ListView inboxList;
		TaskAdapter taskAdapter;

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			rootView = inflater.Inflate (Resource.Layout.MainInbox, container, false);

			inboxList = rootView.FindViewById<ListView> (Resource.Id.InboxTaskList);
			taskAdapter = new TaskAdapter (inflater, ViewModel.Tasks);
			inboxList.Adapter = taskAdapter;

			inboxList.ItemClick += (sender, e) => {
				ViewModel.SelectTask(e.Position);
				if(ViewModel.SelectedTask != null) {
					Intent intent = new Intent(rootView.Context, typeof(TaskEditActivity));
					StartActivity(intent);
				}
			};

			inboxList.ItemLongClick += (object sender, AdapterView.ItemLongClickEventArgs e) => {
				ViewModel.SelectTask(e.Position);
				ViewModel.StartSelectedTask();
				ViewModel.UnselectTask ();
				Activity.ActionBar.SetSelectedNavigationItem(0);
				Activity.InvalidateOptionsMenu();
			};

			return rootView;
		}		
	}
}

