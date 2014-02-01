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

		private static Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

		public static string ReadAssetString(string name)
		{
			string content;
			using (var st = Application.Context.ApplicationContext.Assets.Open (name))
			using (var sr = new StreamReader (st))
				content = sr.ReadToEnd ();
//			using (StreamReader sr = new StreamReader (Application.Context.ApplicationContext.Assets.Open (name)))
//				content = sr.ReadToEnd ();

			return content;
		}

		public static string ReadString(BinaryReader br, int num)
		{
			byte [] buf = br.ReadBytes(num);
			for(int i = 0; i < num; i++) {
				if(buf[i] == 0) {
					if(i == 0) {
						return null;
					} else {
						var r = new byte[i];
						for(int j = 0; j < i; j++) {
							r[j] = buf[j];
						}

						return sjisEnc.GetString (r);
					}	
				}
			}

			var rn = new byte[num + 1];
			for(int i = 0; i < num; i++) {
				rn [i] = buf [i];
			}
			rn [num] = (byte)'\0';

			return sjisEnc.GetString (rn, 0, num);
		}

		public static float[] ReadFloats(BinaryReader br, int num)
		{
			float[] tmp = new float[num];
			for (int i = 0; i < num; i++) {
				tmp [i] = br.ReadSingle ();
			}

			return tmp;
		}

		public static void EnsureDirectory (string str)
		{
			if (str != System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal) && !Directory.Exists (str)) {
				EnsureDirectory (System.IO.Path.GetDirectoryName(str));
				Directory.CreateDirectory (str);
			}
		}
	}
}

