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
using Android.Util;

using Ruly.viewmodel;

namespace Ruly.view
{
	//	[Activity (Label = "Ruly.Shell", MainLauncher = true, Theme="@android:style/Theme.Holo.Light.NoActionBar")]			
	[Activity (Label = "Ruly.Shell", MainLauncher = true)]			
	public class ShellActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			RequestWindowFeature (WindowFeatures.ActionBarOverlay);
			SetContentView (Resource.Layout.ShellActivity);

//			FragmentManager.BeginTransaction().Add(Resource.Layout.ShellFragment, new ShellFragment()).Commit();
//			FragmentManager.BeginTransaction ().Replace (Resource.Id.ShellActivityFrame, new ShellFragment ()).Commit ();
			var fm = FragmentManager.BeginTransaction ();
			fm.Add (Resource.Id.ShellActivityFrame, new ShellFragment ());
			fm.Commit ();

		}

		protected override void OnStart ()
		{
			base.OnStart ();
			AlarmReceiver.SetAlarm ();
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.shell_menu, menu);
			return true;
		}

		public override bool OnTouchEvent (MotionEvent e)
		{
			if (e.Action == MotionEventActions.Up && e.DownTime >= 1000) {
				StartActivity (typeof(MainActivity));
			}
			return true;
		}

	}
}

