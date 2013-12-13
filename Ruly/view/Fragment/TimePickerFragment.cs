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
using System.ComponentModel;

using Ruly.viewmodel;

namespace Ruly.view
{
	public class TimePickerFragment : Android.Support.V4.App.DialogFragment  {

		TimePickerDialog.IOnTimeSetListener timeSetListner;

		public TimePickerFragment(TimePickerDialog.IOnTimeSetListener l)
		{
			timeSetListner = l;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState) {
			int hour = DateTime.Now.Hour;
			int minute = DateTime.Now.Minute;

			// Create a new instance of DatePickerDialog and return it
			return new TimePickerDialog (Activity, timeSetListner, hour, minute, true);
		}
	}
}

