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

namespace ruly.view
{
	[Activity (Label = "ShellActivity", MainLauncher = true, Theme="@android:style/Theme.Holo.Light.NoActionBar")]			
	public class ShellActivity : Activity
	{
		ShellView shellView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			shellView = new ShellView (this);
			SetContentView (shellView);
		}
	}
}

