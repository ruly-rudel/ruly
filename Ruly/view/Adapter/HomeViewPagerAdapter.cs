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
using Android.Support.V4.View;

namespace Ruly.view
{
	public class HomeViewPagerAdapter : FragmentPagerAdapter
	{
		public HomeViewPagerAdapter(Android.Support.V4.App.FragmentManager fm) : base(fm) { }

		#region implemented abstract members of PagerAdapter
		public override int Count {
			get {
//				return 3;
				return 2;
			}
		}
		#endregion
		#region implemented abstract members of FragmentPagerAdapter
		public override Android.Support.V4.App.Fragment GetItem (int position)
		{
			if (position == 0) {
				return new TodayFragment ();
			} else if (position == 1) {
				return new InboxFragment ();
			} else {
//				return new NextFragment ();
				return null;
			}
		}
		#endregion
	}
}

