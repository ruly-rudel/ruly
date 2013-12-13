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
using System.Collections.ObjectModel;


using Ruly.viewmodel;
using Ruly.model;

namespace Ruly.view
{
	public class TaskEditFragment : Android.Support.V4.App.Fragment, TimePickerDialog.IOnTimeSetListener
	{
		View rootView;
		EditText taskTitle;
		EditText taskNote;
		ListView taskAlarmList;
		AlarmAdapter alarmAdapter;
		ObservableCollection<TaskAlarm> alarmList;
		int currentAlarmPosition;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetHasOptionsMenu (true);
			alarmList = ViewModel.SelectedTaskAlarm;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			rootView = inflater.Inflate (Resource.Layout.TaskEdit, container, false);

			taskTitle = rootView.FindViewById<EditText> (Resource.Id.taskTitle);
			taskNote = rootView.FindViewById<EditText> (Resource.Id.taskNote);

			taskAlarmList = rootView.FindViewById<ListView> (Resource.Id.taskAlarmList);
			alarmAdapter = new AlarmAdapter (inflater, alarmList);
			taskAlarmList.Adapter = alarmAdapter;

			taskAlarmList.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {
//				Toast.MakeText(Activity, "hoge", ToastLength.Long);
				currentAlarmPosition = e.Position;
				var df = new TimePickerFragment(this);
				df.Show(this.FragmentManager, "timePicker");
			};

			taskTitle.Text = ViewModel.SelectedTask.Title;
			taskNote.Text = ViewModel.SelectedTask.Note;

			// after text changed: viewmodel sync
			taskTitle.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => { ViewModel.SelectedTask.Title = taskTitle.Text; };
			taskNote.AfterTextChanged += (object sender, Android.Text.AfterTextChangedEventArgs e) => { ViewModel.SelectedTask.Note = taskNote.Text; };

			return rootView;
		}

		public override void OnCreateOptionsMenu (IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate (Resource.Menu.edit_menu, menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case Resource.Id.menu_discard:
				ViewModel.DiscardTask ();
				Activity.Finish ();
				return true;

			case Resource.Id.menu_save:
				if (ViewModel.SelectedTask.CreateDateTime == new DateTime ()) {
					ViewModel.SelectedTask.CreateDateTime = DateTime.Now;
				}
				ViewModel.SelectedTask.LastModifiedDateTime = DateTime.Now;

				ViewModel.CommitTask ();
				Activity.Finish ();
				return true;

			case Resource.Id.menu_add:
				ViewModel.AddAlarm (DateTime.Now.Hour, DateTime.Now.Minute);
				return true;


			default:
				return false;
			}
		}

		public void OnTimeSet (TimePicker view, int hourOfDay, int minute)
		{
			alarmList [currentAlarmPosition].AlarmDateTime = new DateTime (1970, 1, 1, hourOfDay, minute, 0);
			ViewModel.Update (alarmList [currentAlarmPosition]);
		}
	}
	
}

