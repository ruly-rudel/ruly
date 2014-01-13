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

using Ruly.model;
using Android.Opengl;
using Android.Util;
using System.IO;
using Java.Util.Zip;


namespace Ruly.viewmodel
{
	public class ShellViewModel
	{
		private static ShellViewModel me = new ShellViewModel ();
		private List<Shell> shells = new List<Shell>();
		private ShellViewModelData data = new ShellViewModelData();
		private float[]				mPMatrix = new float[16];
		private float[] location = new float[3];
		private float[] rotation = new float[3];

		private bool loaded = false;

		private object lockobj = new object();


		public ShellViewModel()
		{
		}
		
		public static int Width {
			get;
			set;
		}

		public static int Height {
			get;
			set;
		}

		// the number of bones
		public static int MaxBone {
			get;
			set;
		}

		public static float[] ProjectionMatrix {
			get {
				return me.mPMatrix;
			}
		}

		public static List<Shell> Shells {
			get { return me.shells; }
		}

		public static ShellViewModelData Data {
			get { return me.data; }
		}

		public static Shell LoadPMD (string root, string dir, string name)
		{
			var shell = new Shell ();
			shell.LoadPMD (root, dir, name);
			lock (me.lockobj) {
				Shells.Add (shell);
			}
			return shell;
		}

		public static Shell LoadPMF (string root, string dir, string name)
		{
			var shell = new Shell ();
			shell.LoadPMF (root, dir, name);
			lock (me.lockobj) {
				Shells.Add (shell);
			}
			return shell;
		}

		public static void LoadVMD (string root, string dir, string name)
		{
			lock (me.lockobj) {
				Shells [0].LoadVMD (root, dir, name);
			}
		}

		public static void CommitShell ()
		{
			lock (me.lockobj) {
				foreach (var i in Shells) {
					i.Loaded = true;
				}
				me.loaded = true;
			}
		}

		public static void LockWith (Action act)
		{
			lock (me.lockobj) {
				act ();
			}
		}

		public static bool Loaded {
			get { return me.loaded; }
			private set {
				me.loaded = value;
			}
		}

		public static void Animate ()
		{
			// motion
			var ts = (DateTime.Now - new DateTime (0)).TotalSeconds * 60.0;
			lock (me.lockobj) {
				foreach (var i in Shells) {
					if (i.Loaded && i.Surface.Animation) {
						double fr = ts % i.Motions [i.CurrentMotion].max_frame;
						i.MoveBoneAtFrame ((float)fr);
					}
				}
			}

			// set time
			Data.TodayTime = DateTime.Now.ToLongTimeString();
			Data.TodayDate = DateTime.Today.ToShortDateString ();
			Data.TodayDay = DateTime.Today.DayOfWeek.ToString ();
		}

		public static void setCamera(float d, float[] pos, float[] rot, float angle, int width, int height) {
			// Projection Matrix
			float s = (float) Math.Sin(angle * Math.PI / 360);
			Matrix.SetIdentityM(me.mPMatrix, 0);
//			if (mAngle == 90) {
//				Matrix.FrustumM(mPMatrix, 0, -s, s, -s * height / width, s * height / width, 1f, 3500f);
//			} else {
//				Matrix.FrustumM(mPMatrix, 0, -s * width / height, s * width / height, -s, s, 1f, 3500f);
//			}
			Matrix.FrustumM(me.mPMatrix, 0, -s * width / height, s * width / height, -s, s, 1f, 3500f);

			Matrix.ScaleM(me.mPMatrix, 0, 1, 1, -1); // to right-handed
//			Matrix.RotateM(mPMatrix, 0, mAngle, 0, 0, -1); // rotation

//			Matrix.MultiplyMM(mPMatrix, 0, mPMatrix, 0, mRMatrix, 0);	// device rotation

			// camera
			Matrix.TranslateM(me.mPMatrix, 0, 0, 0, -d);
			Matrix.RotateM(me.mPMatrix, 0, rot[2], 0, 0, 1f);
			Matrix.RotateM(me.mPMatrix, 0, rot[0], 1f, 0, 0);
			Matrix.RotateM(me.mPMatrix, 0, rot[1], 0, 1f, 0);
			Matrix.TranslateM(me.mPMatrix, 0, -pos[0], -pos[1], -pos[2]);
		}

		public static void setDefaultCamera() {
//			Matrix.SetIdentityM(mRMatrix, 0);
			me.location[0] = 0;
			me.location[1] = 15f; // 10
			me.location[2] = 0;
			me.rotation[0] = 0;
			me.rotation[1] = 0;
			me.rotation[2] = 0;
			setCamera(-15f, me.location, me.rotation, 45, Width, Height); // -38f
//			setCamera(-38f, me.location, me.rotation, 45, Width, Height); // -15f
//			if (mAngle == 0) {
//				mCameraIndex.location[0] = 0;
//				mCameraIndex.location[1] = 10; // 13
//				mCameraIndex.location[2] = 0;
//				mCameraIndex.rotation[0] = 0;
//				mCameraIndex.rotation[1] = 0;
//				mCameraIndex.rotation[2] = 0;
//				setCamera(-35f, mCameraIndex.location, mCameraIndex.rotation, 45, mWidth, mHeight); // -38f
//			} else {
//				mCameraIndex.location[0] = 0;
//				mCameraIndex.location[1] = 10;
//				mCameraIndex.location[2] = 0;
//				mCameraIndex.rotation[0] = 0;
//				mCameraIndex.rotation[1] = 0;
//				mCameraIndex.rotation[2] = 0;
//				setCamera(-30f, mCameraIndex.location, mCameraIndex.rotation, 45, mWidth, mHeight);
//			}
		}

		public static void SetupShell(string target)
		{
			// Unzip
			var root = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			if(!Directory.Exists(root + "/" + target)) {
				Log.Debug ("Ruly", "extracting default Shell...");
				using (var s = Application.Context.Assets.Open ("Shell/" + target + ".zip")) 
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

