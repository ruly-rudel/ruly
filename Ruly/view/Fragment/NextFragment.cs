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
	public class NextFragment : Android.Support.V4.App.Fragment
	{
		View rootView;
		ListView nextList;
		TaskAdapter taskAdapter;

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			rootView = inflater.Inflate (Resource.Layout.MainNext, container, false);

			nextList = rootView.FindViewById<ListView> (Resource.Id.NextTaskList);
			taskAdapter = new TaskAdapter (inflater, ViewModel.Tasks);
			nextList.Adapter = taskAdapter;

			nextList.ItemClick += (sender, e) => {
				ViewModel.SelectTask(e.Position);
				if(ViewModel.SelectedTask != null) {
					Intent intent = new Intent(rootView.Context, typeof(TaskInfoActivity));
					StartActivity(intent);
				}	
			};

			return rootView;
		}
	}
}
