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
using Ruly.model;
using System.Collections.ObjectModel;

using Ruly.viewmodel;

namespace Ruly.view
{
	public class TaskHistryAdapter: BaseAdapter<CombinedTaskHistory>
	{
		ObservableCollection<CombinedTaskHistory> items;
		ObservableCollection<TaskData> tasks;
		LayoutInflater inflater;
		public TaskHistryAdapter(LayoutInflater inflater, ObservableCollection<CombinedTaskHistory> items, ObservableCollection<TaskData> tasks) : base() {
			this.inflater = inflater;
			this.items = items;
			this.tasks = tasks;

			this.items.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => { NotifyDataSetChanged (); };
			this.tasks.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => { NotifyDataSetChanged (); };
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override CombinedTaskHistory this[int position] {  
			get { return items[position]; }
		}

		public override int Count {
			get { 
				return items.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) // otherwise create a new one
				view = inflater.Inflate(Resource.Layout.TaskHistoryItem, null);

			SetTaskData (view, position);
			SetData (view, position);

			items [position].PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
				SetData (view, position);
			};
			ViewModel.GetTaskFromId(items[position].TaskId[0]).PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
				SetTaskData (view, position);
			};

			return view;
		}

		private void SetData(View view, int position)
		{
			if (items [position].TaskId [0] != -1) {
				view.FindViewById<TextView> (Resource.Id.taskHistoryFrom).Text = items [position].Begin.ToShortTimeString ();
				view.FindViewById<TextView> (Resource.Id.taskHistoryName).SetBackgroundColor (Android.Graphics.Color.Rgb (186, 117, 220));
				if (items [position].End == new DateTime ()) {
					view.FindViewById<TextView> (Resource.Id.taskHistoryTo).Text = "current";
					var span = DateTime.Now - items [position].Begin;
					view.SetMinimumHeight ((int)span.TotalMinutes * 3); 
				} else {
					view.FindViewById<TextView> (Resource.Id.taskHistoryTo).Text = items [position].End.ToShortTimeString ();
					var span = items [position].End - items [position].Begin;
					view.SetMinimumHeight ((int)span.TotalMinutes * 3); 
				}
			} else {
				view.FindViewById<TextView> (Resource.Id.taskHistoryFrom).Text = "";
				view.FindViewById<TextView> (Resource.Id.taskHistoryTo).Text = "";
				view.FindViewById<TextView>(Resource.Id.taskHistoryName).SetBackgroundColor (Android.Graphics.Color.White);
				if (items [position].End == new DateTime ()) {
					var span = DateTime.Now - items [position].Begin;
					view.SetMinimumHeight ((int)span.TotalMinutes * 3); 
				} else {
					var span = items [position].End - items [position].Begin;
					view.SetMinimumHeight ((int)span.TotalMinutes * 3); 
				}
			}
		}

		private void SetTaskData(View view, int position)
		{
			view.FindViewById<TextView> (Resource.Id.taskHistoryName).Text = ViewModel.GetTaskFromId(items [position].TaskId[0]).Title;
			if (items [position].TaskId.Count > 1) {	// has more tasks
				for(int i = 1; i < items[position].TaskId.Count; i++) {
					view.FindViewById<TextView> (Resource.Id.taskHistoryName).Text += ", " + ViewModel.GetTaskFromId (items [position].TaskId [i]).Title;
				}
			}
		}
	}
}

