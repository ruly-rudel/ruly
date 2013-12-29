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

namespace Ruly.viewmodel
{
	public class ShellViewModel
	{
		private static ShellViewModel me = new ShellViewModel ();
		private List<Shell> shells = new List<Shell>();
		private float[]				mPMatrix = new float[16];
		public float[] location = new float[3];
		public float[] rotation = new float[3];

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
		public static int GLConfig {
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

		public static Shell LoadPMD (string root, string dir, string name)
		{
			var shell = new Shell ();
			shell.LoadPMD (root, dir, name);
			Shells.Add(shell);
			return shell;
		}

		public static void LoadVMD (string root, string dir, string name)
		{
			Shells[0].LoadVMD(root, dir, name);
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
			me.location[1] = 10; // 13
			me.location[2] = 0;
			me.rotation[0] = 0;
			me.rotation[1] = 0;
			me.rotation[2] = 0;
			setCamera(-35f, me.location, me.rotation, 45, Width, Height); // -38f
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
	}
}

