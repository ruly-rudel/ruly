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
	public class AddTaskFragment : Android.Support.V4.App.DialogFragment
	{
		EditText editText;

		public override Dialog OnCreateDialog (Bundle savedInstanceState)
		{
			editText = new EditText (Activity);
			var builder = new AlertDialog.Builder (Activity);
			builder.SetTitle ("Add Task")
				.SetView (editText)
				.SetPositiveButton ("Go", (e, v) => {
					ViewModel.SelectedTask.Title = editText.Text;
					if (ViewModel.SelectedTask.CreateDateTime == new DateTime ()) {
						ViewModel.SelectedTask.CreateDateTime = DateTime.Now;
					}
					ViewModel.SelectedTask.LastModifiedDateTime = DateTime.Now;

					ViewModel.CommitTask ();
					ViewModel.StartSelectedTask ();
				})
				.SetNeutralButton("Save", (e, v) => { 
					ViewModel.SelectedTask.Title = editText.Text;
					if (ViewModel.SelectedTask.CreateDateTime == new DateTime ()) {
						ViewModel.SelectedTask.CreateDateTime = DateTime.Now;
					}
					ViewModel.SelectedTask.LastModifiedDateTime = DateTime.Now;

					ViewModel.CommitTask ();
					ViewModel.UnselectTask();
				})
				.SetNegativeButton("Cancel", (e, v) => { ViewModel.UnselectTask(); });

			return builder.Create ();
		}

		public override void OnDismiss (IDialogInterface dialog)
		{
			base.OnDismiss (dialog);
			Activity.InvalidateOptionsMenu ();
		}
	}
}

