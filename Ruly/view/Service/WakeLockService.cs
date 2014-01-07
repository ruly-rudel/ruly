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

namespace Ruly.view
{
	[Service]
	public class WakeLockService : Service
	{
		public override void OnCreate ()
		{
			base.OnCreate ();
			// PowerManager
			var power = (PowerManager)GetSystemService (Context.PowerService);
			var wl = power.NewWakeLock (WakeLockFlags.Full | WakeLockFlags.AcquireCausesWakeup | WakeLockFlags.OnAfterRelease, "Ruly.Ruly");
			wl.Acquire ();
			wl.Release ();

			// KeyguardLock
			var km = (KeyguardManager) GetSystemService(Context.KeyguardService);
			var klock = km.NewKeyguardLock("Ruly.Ruly");
			klock.DisableKeyguard();

			var i = new Intent(this, typeof(ShellActivity));
			i.SetFlags(ActivityFlags.NewTask | ActivityFlags.NoUserAction);
		}

		#region implemented abstract members of Service
		public override IBinder OnBind (Intent intent)
		{
			return null;
		}
		#endregion
	}
}

