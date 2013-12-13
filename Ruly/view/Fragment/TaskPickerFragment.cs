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

using Ruly.viewmodel;

namespace Ruly.view
{
	public class TaskPickerFragment : Android.Support.V4.App.DialogFragment, IDialogInterfaceOnCancelListener
	{
		public override Dialog OnCreateDialog (Bundle savedInstanceState)
		{
			var builder = new AlertDialog.Builder (Activity);
			builder.SetTitle ("Replace Task")
				.SetAdapter (new TaskAdapter (Activity.LayoutInflater, ViewModel.Tasks), (object sender, DialogClickEventArgs e) => {
				ViewModel.SelectedRawTaskHistory.TaskId [0] = ViewModel.Tasks [e.Which].Id;
				ViewModel.CommitRawTaskHistory ();
				})
				.SetOnCancelListener (this);

			return builder.Create ();
		}

		public override void OnCancel (IDialogInterface dialog)
		{
			base.OnCancel (dialog);
			ViewModel.DiscardRawTaskHistory ();
		}
	}
}

