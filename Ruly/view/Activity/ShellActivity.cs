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
	[Activity (Label = "ShellActivity", MainLauncher = true, Theme="@android:style/Theme.Holo.Light.NoActionBar")]			
	public class ShellActivity : Activity
	{
		ShellView shellView;

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

			SetupShell ();
//			ShellViewModel.LoadPMD ("Shell/mikuXS/mikuXS.pmd");
			ShellViewModel.LoadPMD (root, "/default", "/miku.pmd");
//			ShellViewModel.LoadPMD (root, "/default", "/miku1052C-Re.pmd");

			shellView = new ShellView (this);
			SetContentView (shellView);
		}

		private void SetupShell()
		{
			var root = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			if(!Directory.Exists(root + "/default")) {
				Log.Debug ("Ruly", "extracting default Shell...");
				using (var s = Assets.Open ("Shell/default.zip")) 
				using (var z = new ZipInputStream(s))
				{
					ZipEntry ze;
					while ((ze = z.NextEntry) != null) {
						if (!ze.IsDirectory) {
							string name = root + "/" + ze.Name;
							EnsureDirectory (System.IO.Path.GetDirectoryName(name));
							using (var ws = File.OpenWrite (name)) 
							{
								for (int i = 0; i < ze.Size; i++) {
									ws.WriteByte ((byte)z.Read());
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

