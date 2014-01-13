using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Ruly.viewmodel;
using System.Threading.Tasks;

namespace Ruly.view
{
	public class ShellFragment : Fragment
	{
		TextView todayDate;
		TextView todayDay;
		TextView todayTime;
		Handler handle = new Handler();

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var root = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);

			if (!ShellViewModel.Loaded) {
				Task.Run (() => {
					#if MIKU1052
					ShellViewModel.SetupShell ("default");
					ShellViewModel.LoadPMD (root, "/default", "/miku1052C-Re.pmd");
					#elif MIKU
					ShellViewModel.SetupShell ("default");
					ShellViewModel.LoadPMD (root, "/default", "/miku.pmd");
					#elif LAT
					ShellViewModel.SetupShell ("Lat");
					ShellViewModel.LoadPMD (root, "/Lat", "/LatVer2.3_White.pmd");
					#elif XSc
					ShellViewModel.SetupShell ("mikuXSc");
//				    ShellViewModel.ShellViewModel.LoadPMD (root, "/mikuXSc", "/mikuXS.pmd");
					ShellViewModel.LoadPMF (root, "/mikuXSc", "/mikuXS.pmf");
					#elif XS
					ShellViewModel.SetupShell("mikuXS");
					ShellViewModel.LoadPMD(root, "/mikuXS", "/mikuXS.pmd");
					#endif
					ShellViewModel.LoadVMD (root, "/motion", "/stand_pose.vmd");
					ShellViewModel.CommitShell ();
				});
			} else {
				TextureFile.Reset ();
			}

			var rootView = inflater.Inflate (Resource.Layout.ShellFragment, container, false);
			todayDate = rootView.FindViewById<TextView> (Resource.Id.TodayDate);
			todayDay  = rootView.FindViewById<TextView> (Resource.Id.TodayDay);
			todayTime = rootView.FindViewById<TextView> (Resource.Id.TodayTime);

			ShellViewModel.Data.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
				handle.Post(() => {
					todayDate.Text = ShellViewModel.Data.TodayDate;
					todayDay.Text = ShellViewModel.Data.TodayDay;
					todayTime.Text = ShellViewModel.Data.TodayTime;
				});
			};

			return rootView;
		}
	}
}

