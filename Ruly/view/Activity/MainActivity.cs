using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Ruly.view;
using Ruly.model;
using Java.Util;
using Android.Support.V4.App;
using Android.Support.V4.View;

using Ruly.viewmodel;
using Android.Util;

namespace Ruly.view
{
	[Activity (Label = "Ruly", UiOptions = Android.Content.PM.UiOptions.SplitActionBarWhenNarrow)]
	public class MainActivity : Android.Support.V4.App.FragmentActivity, ActionBar.ITabListener, ViewPager.IOnPageChangeListener
	{
		ViewPager viewPager;
		HomeViewPagerAdapter homeViewPagerAdapter;


		// Lifecycle
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			ViewModel.LoadState ();

			SetContentView (Resource.Layout.Main);

			homeViewPagerAdapter = new HomeViewPagerAdapter (SupportFragmentManager);
			viewPager = FindViewById<ViewPager> (Resource.Id.viewPagerHome);
			viewPager.Adapter = homeViewPagerAdapter;
			viewPager.SetOnPageChangeListener (this);

			// Action Bar with tabs
			ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
			ActionBar.AddTab(ActionBar.NewTab ().SetText ("Today").SetTabListener (this));
			ActionBar.AddTab(ActionBar.NewTab ().SetText ("Inbox").SetTabListener (this));

		}

		protected override void OnStart ()
		{
			base.OnStart ();
			AlarmReceiver.SetAlarm ();
		}




		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.main_menu, menu);
			return true;
		}

		public override bool OnPrepareOptionsMenu (IMenu menu)
		{
			menu.GetItem (4).SetVisible (true);	// add
			switch (ViewModel.State) {
			case 0:	// idle
				menu.GetItem (0).SetVisible (false);	// go
				menu.GetItem (1).SetVisible (false);	// pause
				menu.GetItem (2).SetVisible (false);	// resume
				menu.GetItem (3).SetVisible (false);	// finish
				break;

			case 1:	// go
				menu.GetItem (0).SetVisible (false);	// go
				menu.GetItem (1).SetVisible (true);		// pause
				menu.GetItem (2).SetVisible (false);	// resume
				menu.GetItem (3).SetVisible (true);		// finish
				break;

			case 2:	// pause
				menu.GetItem (0).SetVisible (false);	// go
				menu.GetItem (1).SetVisible (false);	// pause
				menu.GetItem (2).SetVisible (true);		// resume
				menu.GetItem (3).SetVisible (true);		// finish
				break;

			default:
				throw new NotImplementedException ();
			}

			return true;
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
			InvalidateOptionsMenu ();	// ad-hock change: it may be poor performance
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			ViewModel.SaveState ();
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) {
			case Resource.Id.menu_finish:
				ViewModel.Finish ();
				InvalidateOptionsMenu ();
				return true;

			case Resource.Id.menu_pause:
				ViewModel.Pause ();
				InvalidateOptionsMenu ();
				return true;

			case Resource.Id.menu_resume:
				ViewModel.Resume ();
				InvalidateOptionsMenu ();
				return true;

			case Resource.Id.menu_new:
				ViewModel.PrepareNewTask ();
				var addTaskFragment = new AddTaskFragment ();
				addTaskFragment.Show (SupportFragmentManager, "addTask");
//				Intent intent = new Intent (this, typeof(TaskEditActivity));
//				StartActivity (intent);
				return true;

			default: 
				return base.OnOptionsItemSelected(item);
			}
		}

		// for ActionBar.ITabListener
		public void OnTabReselected (ActionBar.Tab tab, Android.App.FragmentTransaction ft)
		{
		}

		public void OnTabSelected (ActionBar.Tab tab, Android.App.FragmentTransaction ft)
		{
			viewPager.SetCurrentItem (tab.Position, true);
		}

		public void OnTabUnselected (ActionBar.Tab tab, Android.App.FragmentTransaction ft)
		{
		}

		// for ViewPager.IOnPageChangeListener
		public void OnPageScrollStateChanged (int state)
		{
		}

		public void OnPageScrolled (int position, float positionOffset, int positionOffsetPixels)
		{
		}

		public void OnPageSelected (int position)
		{
			ActionBar.SetSelectedNavigationItem(position);
		}
	}
}


