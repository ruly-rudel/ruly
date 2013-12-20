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
using System.IO;

namespace Ruly
{
	public static class Util
	{
		public static string ReadAssetString(string name)
		{
			string content;
			using (StreamReader sr = new StreamReader (Application.Context.ApplicationContext.Assets.Open (name)))
				content = sr.ReadToEnd ();

			return content;
		}
	}
}

