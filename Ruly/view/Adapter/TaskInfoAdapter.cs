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
using Ruly.model;

namespace Ruly.view
{
	public class TaskInfoAdapter : BaseAdapter<string>
	{
		TaskData task;
		LayoutInflater infrater;
		public TaskInfoAdapter(LayoutInflater inflater, TaskData task) : base() {
			this.infrater = inflater;
			this.task = task;

			this.task.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => { NotifyDataSetChanged(); };
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override string this[int position] {
			get {
				switch (position) {
				case 0:
					return "TITLE";

				case 1:
					return task.Title;

				case 2:
					return "NOTIFY";

				default:
					return "";
				}
			}
		}

		public override int Count {
			get {
				return 3;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) // otherwise create a new one
				view = infrater.Inflate(Resource.Layout.TaskItem, null);

			SetData (view, position);

			return view;
		}

		private void SetData(View view, int position)
		{
			var v = view.FindViewById<TextView> (Resource.Id.taskTitle);
			switch (position) {
			case 0:
				v.Text = "TITLE";
				break;

			case 1:
				v.Text = task.Title;
				break;

			case 2:
				v.Text = "NOTIFY";
				break;

			default:
				break;
			}
		}
	}
}

