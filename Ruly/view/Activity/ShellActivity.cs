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
	[Activity (Label = "ShellActivity", MainLauncher = true, Theme="@android:style/Theme.Holo.Light.NoActionBar")]			
	public class ShellActivity : Activity
	{
		ShellView shellView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

//			ShellViewModel.LoadPMD ("Shell/mikuXS/mikuXS.pmd");
			ShellViewModel.LoadPMD ("Shell/default/miku.pmd");
//			ShellViewModel.LoadPMD ("Shell/default/miku1052C-Re.pmd");
			shellView = new ShellView (this);
			SetContentView (shellView);
		}
	}
}

