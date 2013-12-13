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
	public class AlarmAdapter : BaseAdapter<TaskAlarm>
	{
		ObservableCollection<TaskAlarm> alarms;
		LayoutInflater infrater;

		public AlarmAdapter(LayoutInflater inflater, ObservableCollection<TaskAlarm> alarms) : base() {
			this.infrater = inflater;
			this.alarms = alarms;

			this.alarms.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => { NotifyDataSetChanged(); } ;
		}

		#region implemented abstract members of BaseAdapter
		public override long GetItemId (int position)
		{
			return position;
		}
		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View view = convertView; // re-use an existing view, if one is available
			if (view == null) // otherwise create a new one
				view = infrater.Inflate(Resource.Layout.TaskAlarm, null);

			SetData (view, position);
			alarms[position].PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
				SetData(view, position);
			};

			return view;
		}
		public override int Count {
			get {
				return alarms.Count ();
			}
		}
		#endregion
		#region implemented abstract members of BaseAdapter
		public override TaskAlarm this [int index] {
			get {
				return alarms [index];
			}
		}
		#endregion

		private void SetData(View view, int position)
		{
			view.FindViewById<TextView> (Resource.Id.taskAlarmDateTime).Text = alarms[ position ].AlarmDateTime.ToShortTimeString();
		}
	}
}

