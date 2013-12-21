using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Android.Views;

using Android.Widget;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

using Ruly.viewmodel;

namespace Ruly.view
{
	public class ShellView : GLSurfaceView
	{
		ShellRenderer shellRenderer;

		public ShellView (Context context) :
		base (context)
		{
			Initialize ();
		}

		public ShellView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}

		void Initialize ()
		{
			SetEGLContextClientVersion (2);
			shellRenderer = new ShellRenderer ();
			SetRenderer (shellRenderer);
		}
	}
}

