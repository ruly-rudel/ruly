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

using Android.Support.V4.App;
using Android.Support.V4.View;

using Ruly.viewmodel;

namespace Ruly.view
{
	public class DatePickerFragment : Android.Support.V4.App.DialogFragment, DatePickerDialog.IOnDateSetListener {
		TextView tv;

		public DatePickerFragment(TextView tv) : base()
		{
			this.tv = tv;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState) {
			int year = DateTime.Now.Year;
			int month = DateTime.Now.Month;
			int day = DateTime.Now.Day;

			// Create a new instance of DatePickerDialog and return it
			return new DatePickerDialog(Activity, this, year, month, day);
		}

		public void OnDateSet(DatePicker view, int year, int month, int day) {
			ViewModel.SelectedTask.DueDateTime = new DateTime (year, month + 1, day, 
				ViewModel.SelectedTask.DueDateTime.Hour,
				ViewModel.SelectedTask.DueDateTime.Minute,
				0);
			tv.Text = ViewModel.SelectedTask.DueDateTime.ToLongDateString ();
		}
	}
}

