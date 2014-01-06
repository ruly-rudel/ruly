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
using System.IO;
using Java.Util.Zip;
using System.Threading.Tasks;

namespace Ruly.view
{
	[Activity (Label = "Ruly.Shell", MainLauncher = true, Theme="@android:style/Theme.Holo.Light.NoActionBar")]			
	public class ShellActivity : Activity
	{
//		TextView textView;
		TextView todayDate;
		TextView todayDay;
		TextView todayTime;
		Handler handle = new Handler();

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var root = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);

//			if (!Directory.Exists (root + "/default")) {
//				Log.Debug ("Ruly.dirdoesnotexist", root + "/default");
//			} else {
//				Log.Debug ("Ruly.dir", root);
//				foreach (var i in Directory.GetFiles(root + "/default"))
//					Log.Debug("Ruly.dirList", i);
//			}

			if (!ShellViewModel.Loaded) {
				Task.Run (() => {
					#if MIKU1052
					SetupShell ("default");
					ShellViewModel.LoadPMD (root, "/default", "/miku1052C-Re.pmd");
					#elif MIKU
					SetupShell ("default");
					ShellViewModel.LoadPMD (root, "/default", "/miku.pmd");
					#elif LAT
					SetupShell ("Lat");
					ShellViewModel.LoadPMD (root, "/Lat", "/LatVer2.3_White.pmd");
					#elif XSc
					SetupShell ("mikuXSc");
//				    ShellViewModel.LoadPMD (root, "/mikuXSc", "/mikuXS.pmd");
					ShellViewModel.LoadPMF (root, "/mikuXSc", "/mikuXS.pmf");
					#elif XS
					SetupShell("mikuXS");
					ShellViewModel.LoadPMD(root, "/mikuXS", "/mikuXS.pmd");
					#endif
					ShellViewModel.LoadVMD (root, "/motion", "/stand_pose.vmd");
					ShellViewModel.CommitShell ();
				});
			} else {
				TextureFile.Reset ();
			}

			SetContentView (Resource.Layout.ShellActivity);
			todayDate = FindViewById<TextView> (Resource.Id.TodayDate);
			todayDay  = FindViewById<TextView> (Resource.Id.TodayDay);
			todayTime = FindViewById<TextView> (Resource.Id.TodayTime);
			ShellViewModel.Data.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
				handle.Post(() => {
					todayDate.Text = ShellViewModel.Data.TodayDate;
					todayDay.Text = ShellViewModel.Data.TodayDay;
					todayTime.Text = ShellViewModel.Data.TodayTime;
				});
			};

//			textView = FindViewById<TextView> (Resource.Id.ShellFrameTitle);
//			textView.Text = ShellViewModel.Shells[0].Surface.ModelName;
		}

		protected override void OnStart ()
		{
			base.OnStart ();
			AlarmReceiver.SetAlarm ();
		}

		public override bool OnTouchEvent (MotionEvent e)
		{
			if (e.Action == MotionEventActions.Up && e.DownTime >= 1000) {
				StartActivity (typeof(MainActivity));
			}
			return true;
		}

		private void SetupShell(string target)
		{
			// Unzip
			var root = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			if(!Directory.Exists(root + "/" + target)) {
				Log.Debug ("Ruly", "extracting default Shell...");
				using (var s = Assets.Open ("Shell/" + target + ".zip")) 
				using (var z = new ZipInputStream(s))
				{
					ZipEntry ze;
					byte[] buf = new byte[1024];
					while ((ze = z.NextEntry) != null) {
						if (!ze.IsDirectory) {
							string name = root + "/" + ze.Name;
							Util.EnsureDirectory (System.IO.Path.GetDirectoryName(name));
							using (var ws = File.OpenWrite (name)) 
							{
								var i = 0;
								while (i < ze.Size) {
									int num = z.Read (buf, 0, 1024);
									ws.Write (buf, 0, num);
									i += num;
								}
								z.CloseEntry ();
								Log.Debug ("Ruly", "Extract File " + ze.Name);
							}
						}
					}
				}
			}
		}


	}
}

