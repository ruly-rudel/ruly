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
using Android.Support.V4.App;
using Android.Util;

using Ruly.viewmodel;
using Ruly.model;

namespace Ruly.view
{
	[BroadcastReceiver]
//	[IntentFilter(new string[]{MainActivity.AlarmAction}, Priority = (int)IntentFilterPriority.LowPriority)]
	public class AlarmReceiver : BroadcastReceiver
	{
		public const string AlarmAction = "Alarm";
		private static Intent alarmIntent = new Intent(Application.Context.ApplicationContext, typeof(AlarmReceiver));

		public static void SetAlarm()
		{
			var alarm = (AlarmManager)Application.Context.ApplicationContext.GetSystemService (Context.AlarmService);

			var alarms = ViewModel.CurrentAlarms;
			foreach (var x in alarms) {
				alarmIntent.SetAction (x.Id.ToString ());
				var pendingAlarmIntent = PendingIntent.GetBroadcast(Application.Context.ApplicationContext, x.Id, alarmIntent, PendingIntentFlags.CancelCurrent);
				alarm.Set (AlarmType.RtcWakeup, ToAndroidTime (x.AlarmDateTime), pendingAlarmIntent);
				Log.Debug ("Ruly", "Alarm Id " + x.Id.ToString () + ", TaskId = " + x.TaskId.ToString () + " is set at " + x.AlarmDateTime.ToLongDateString () + " " + x.AlarmDateTime.ToLongTimeString ());
			}
		}

		private static long ToAndroidTime(DateTime dt)
		{
			DateTime dtBasis = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return (long)dt.ToUniversalTime ().Subtract (dtBasis).TotalMilliseconds;
		}

		private static bool IsAlarmSet ()
		{
			return PendingIntent.GetBroadcast (Application.Context.ApplicationContext, 0, alarmIntent, PendingIntentFlags.NoCreate) != null;
		}

		#region implemented abstract members of BroadcastReceiver

		public override void OnReceive (Context context, Intent intent)
		{
//			var i = new Intent(context, typeof(WakeLockService));
//			context.StartService (i);

			// PowerManager
			var power = (PowerManager)context.GetSystemService (Context.PowerService);
			var wl = power.NewWakeLock (WakeLockFlags.Full | WakeLockFlags.AcquireCausesWakeup | WakeLockFlags.OnAfterRelease, "Ruly.Ruly");
			wl.Acquire ();
//			wl.Release ();

			// KeyguardLock
			var km = (KeyguardManager) context.GetSystemService(Context.KeyguardService);
			var klock = km.NewKeyguardLock("Ruly.Ruly");
			klock.DisableKeyguard();

			var i = new Intent(context, typeof(ShellActivity));
			i.SetFlags(ActivityFlags.NewTask | ActivityFlags.NoUserAction);
			context.StartActivity (i);

//			ShowNotification (context, int.Parse(intent.Action));
		}

		#endregion

		private void ShowNotification(Context context, int id)
		{
			var alarm = ViewModel.GetAlarmFromId (id);
			var task = ViewModel.GetTaskFromId (alarm.TaskId);
			var pendingIntent = PendingIntent.GetActivity (context, 0, new Intent (context, typeof(ShellActivity)), 0);

			var builder = new NotificationCompat.Builder (context);
			builder.SetSmallIcon (Resource.Drawable.Icon);
			builder.SetContentTitle ("Ruly Notification");
			builder.SetContentText (task.Title);
			builder.SetContentIntent (pendingIntent);

			var manager = context.GetSystemService (Context.NotificationService) as NotificationManager;
			manager.Notify (0, builder.Build ());
		}
	}
}

