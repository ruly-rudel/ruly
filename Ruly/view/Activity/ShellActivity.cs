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

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case Resource.Id.menu_alarm:
				return true;

			case Resource.Id.menu_settings:
				var fm = FragmentManager.BeginTransaction();
				fm.Replace(Resource.Id.ShellActivityFrame, new SettingFragment());
				fm.AddToBackStack(null);
				fm.Commit();
				return true;

			default: 
				return base.OnOptionsItemSelected(item);
			}
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

