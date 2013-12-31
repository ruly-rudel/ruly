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

namespace Ruly.view
{
	[Activity (Label = "Ruly.Shell", MainLauncher = true, Theme="@android:style/Theme.Holo.Light.NoActionBar")]			
	public class ShellActivity : Activity
	{
		TextView textView;

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
			SetupShell("mikuXSc");
			ShellViewModel.LoadPMD(root, "/mikuXSc", "/mikuXS.pmd");
			#elif XS
			SetupShell("mikuXS");
			ShellViewModel.LoadPMD(root, "/mikuXS", "/mikuXS.pmd");
			#endif
			ShellViewModel.LoadVMD (root, "/motion", "/stand_pose.vmd");

			SetContentView (Resource.Layout.ShellActivity);
			textView = FindViewById<TextView> (Resource.Id.ShellFrameTitle);
			textView.Text = ShellViewModel.Shells[0].Surface.ModelName;
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
							EnsureDirectory (System.IO.Path.GetDirectoryName(name));
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

		void EnsureDirectory (string str)
		{
			if (str != System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal) && !Directory.Exists (str)) {
				EnsureDirectory (System.IO.Path.GetDirectoryName(str));
				Directory.CreateDirectory (str);
			}
		}
	}
}

