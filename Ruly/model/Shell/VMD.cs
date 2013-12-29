using System;
using System.IO;
using Android.Util;
using System.Collections.Generic;
using System.Linq;

namespace Ruly.model
{
	public class VMD : ShellMotion
	{
		public bool is_vmd {
			get;
			private set;
		}

		public string model_name {
			get;
			private set;
		}

		public VMD (string path)
		{
			max_frame = -1;
			string dir = Path.GetDirectoryName(path);
			Log.Debug ("VMD", "base dir: " + dir);
			using (var fs = File.OpenRead(path)) {
				using (var br = new BinaryReader (fs)) {
					init(br, dir + "/");
				}
			}
		}

		private void init (BinaryReader br, string path)
		{
			parseHeader(br);
			if(is_vmd) {
				parseFrame (br);
				parseFace (br);
			}
		}

		private void parseHeader(BinaryReader br) {
			String magic = Util.ReadString(br, 30);
			Log.Debug("VMD", "MAGIC: " + magic);
			if (magic.Equals("Vocaloid Motion Data 0002")) {
				is_vmd = true;
				model_name = Util.ReadString(br, 20);
			} else {
				is_vmd = false;
			}
		}

		private void parseFrame(BinaryReader br) {
			int num = br.ReadInt32 ();
			Log.Debug ("VMD", "Animation: " + num.ToString());
			string bone_name;

			if (num > 0) {
				// initialize bone hash
				var mh = new Dictionary<string, List<BoneMotion> > ();
				Bone = new Dictionary<string, List<BoneMotion> > ();

				// parse
				for (int i = 0; i < num; i++) {
					BoneMotion m = new BoneMotion ();

					bone_name = Util.ReadString (br, 15);
					m.frame_no = br.ReadInt32 ();
					m.location = Util.ReadFloats (br, 3);
					m.rotation = Util.ReadFloats (br, 4);
					m.interp = br.ReadBytes (16);

					br.BaseStream.Position = br.BaseStream.Position + 48;

					List<BoneMotion> mi;
					if (mh.ContainsKey (bone_name)) {
						mi = mh [bone_name];
					} else {
						mi = new List<BoneMotion> ();

						// set default
						BoneMotion d = new BoneMotion ();
						d.location = new float[3];
						d.rotation = new float[4];

						d.frame_no = -1;
						d.location [0] = d.location [1] = d.location [2] = 0;
						d.rotation [0] = d.rotation [1] = d.rotation [2] = 0;
						d.rotation [3] = 1;
						d.interp = null;
						mi.Add (d);

						mh[bone_name] = mi;
					}

					mi.Add (m);
					if (m.frame_no > max_frame) {
						max_frame = m.frame_no;
					}
				} 

				// sorted by frame_no
				foreach (var i in mh) {
					Bone[i.Key] = new List<BoneMotion>(from x in i.Value orderby x.frame_no select x);
				}
			} else {
				Bone = null;
			}
		}

		private void parseFace(BinaryReader br) {
			int num = br.ReadInt32();
			Log.Debug("VMD", "Face num: " + num.ToString());

			if (num > 0) {
				// initialize face hash
				var mh = new Dictionary<string, List<Morphing> > ();
				Morph = new Dictionary<string, List<Morphing> >();

				// parser
				for (int i = 0; i < num; i++) {
					var m = new Morphing();

					string name = Util.ReadString(br, 15);
					m.frame_no = br.ReadInt32();
					m.weight = br.ReadSingle();

					List<Morphing> fi;
					if(mh.ContainsKey(name)) {
						fi = mh[name];
					} else {
						fi = new List<Morphing>();

						// default face
						var d = new Morphing();
						d.frame_no = -1;
						d.weight = 0;
						fi.Add(d);

						mh[name] = fi;
					}
					fi.Add(m);
				}

				// sorted by frame_no
				foreach (var i in mh) {
					Morph[i.Key] = new List<Morphing>(from x in i.Value orderby x.frame_no select x);
				}

			} else {
				Morph = null;
			}
		}
	}

}

