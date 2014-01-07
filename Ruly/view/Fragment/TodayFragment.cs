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
using System.Timers;

using Ruly.viewmodel;

namespace Ruly.view
{
	public class TodayFragment : Android.Support.V4.App.Fragment
	{
		View rootView;
		TextView todayDate;
		TextView todayDay;
		TextView todayTime;
		ListView historyList;
		ToggleButton toggleAbstDetail;
		TaskHistryAdapter taskHistoryAdapter;

		Timer	timer;
		Handler	handler = new Handler();

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			rootView = inflater.Inflate (Resource.Layout.MainToday, container, false);

			todayDate = rootView.FindViewById<TextView> (Resource.Id.TodayDate);
			todayDay  = rootView.FindViewById<TextView> (Resource.Id.TodayDay);
			todayTime = rootView.FindViewById<TextView> (Resource.Id.TodayTime);
			historyList = rootView.FindViewById<ListView> (Resource.Id.TodayHistoryList);
			historyList.ItemLongClick += (object sender, AdapterView.ItemLongClickEventArgs e) => {
				if(toggleAbstDetail.Checked) {	// at raw state
					ViewModel.SelectRawTaskHistory(e.Position);
					var taskPickerFragment = new TaskPickerFragment();
					taskPickerFragment.Show(Activity.SupportFragmentManager, "taskPicker");
				}
			};

			toggleAbstDetail = rootView.FindViewById<ToggleButton> (Resource.Id.toggleAbstDetail);
			toggleAbstDetail.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => { SetAdapter(); };

			SetAdapter ();

			timer = new Timer ();
			timer.AutoReset = true;
			timer.Interval = 100;	// 100ms
			timer.Elapsed += (object sender, ElapsedEventArgs e) => {
				handler.Post(() =>
					{
						todayTime.Text = DateTime.Now.ToLongTimeString();
						todayDate.Text = ViewModel.ShowDate.ToShortDateString ();
						todayDay.Text = DateTime.Today.DayOfWeek.ToString ();
					});
			};
			timer.Enabled = true;

			todayDate.Touch += (object sender, View.TouchEventArgs e) => {
				if(e.Event.Action == MotionEventActions.Up) {
					var df = new HistoryDatePickerFragment(todayDate);
					df.Show(this.FragmentManager, "historyDatePicker");
				}
			};

			return rootView;
		}

		private void SetAdapter()
		{
			if (toggleAbstDetail.Checked == true) {
				taskHistoryAdapter = new TaskHistryAdapter (Activity.LayoutInflater, ViewModel.RawTaskHistories, ViewModel.Tasks);
				historyList.Adapter = taskHistoryAdapter;
			} else {
				taskHistoryAdapter = new TaskHistryAdapter (Activity.LayoutInflater, ViewModel.TaskHistories, ViewModel.Tasks);
				historyList.Adapter = taskHistoryAdapter;
			}
		}
	}
}

