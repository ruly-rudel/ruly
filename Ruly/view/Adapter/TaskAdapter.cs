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

namespace Ruly.view
{
	public class TaskAdapter: BaseAdapter<TaskData>
	{
		ObservableCollection<TaskData> tasks;
		LayoutInflater infrater;
		public TaskAdapter(LayoutInflater inflater, ObservableCollection<TaskData> tasks) : base() {
			this.infrater = inflater;
			this.tasks = tasks;

			this.tasks.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => { NotifyDataSetChanged(); } ;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override TaskData this[int position] {  
			get { return tasks[position]; }
		}

		public override int Count {
			get {
				return tasks.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) // otherwise create a new one
				view = infrater.Inflate(Resource.Layout.TaskItem, null);

			SetData (view, position);
			tasks[position].PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
				SetData(view, position);
			};

			return view;
		}

		private void SetData(View view, int position)
		{
			view.FindViewById<TextView> (Resource.Id.taskTitle).Text = tasks[ position ].Title;
//			view.FindViewById<TextView> (Resource.Id.taskDueDate).Text = tasks [ position ].DueDateTime.ToLongDateString();
//			view.FindViewById<TextView> (Resource.Id.taskNote).Text = tasks [ position ].Note;
		}

	}
}

