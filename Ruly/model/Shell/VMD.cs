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

		#region implemented abstract members of ShellMotion

		public override BoneMotion BoneMotionAt (RenderBone b, float time)
		{
			if (Bone.ContainsKey (b.bone.name)) {
				var bones = Bone [b.bone.name];
				var m0 = from x in bones
					         where x.frame_no >= time
				     select x;

				if (m0.Count () == 0) {
					return null;
				} else {
					return m0.First ();
				}

//				var m1 = from x in bones
//					     where x.frame_no >= time
//				     select x;
//
//				BoneMotion bm1, bm2;
//
//				if (m0.Count () > 0) {
//					if (m1.Count () > 0) {
//						bm1 = m0.Last;
//						bm2 = m1.First;
//					} else {
//						throw new KeyNotFoundException ();
//					}
//				} else {
//					if (m1.Count () > 0) {
//						bm1 = null;
//						bm2 = m1.First;
//					} else {
//						throw new KeyNotFoundException ();
//					}
//				}
			} else {
				return null;
			}
		}

		#endregion

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


//		private MotionPair findMotion(Bone b, float frame, MotionPair mp) {
//			if (b != null && b.motion != null) {
//				int[] frame_no = b.motion.frame_no;
//				mp.m0 = 0;
//				mp.m1 = b.motion.frame_no.length - 1;
//				if(frame >= frame_no[mp.m1]) {
//					mp.m0 = mp.m1;
//					b.current_motion = mp.m1;
//					mp.m1 = -1;
//					return mp;
//				}
//
//				while(true) {
//					int center = (mp.m0 + mp.m1) / 2;
//					if(center == mp.m0) {
//						b.current_motion = center;
//						return mp;
//					}
//					if(frame_no[center] == frame) {
//						mp.m0 = center;
//						mp.m1 = -1;
//						b.current_motion = center;
//						return mp;
//					} else if(frame_no[center] > frame) {
//						mp.m1 = center;
//					} else {
//						mp.m0 = center;
//					}
//				}
//			}
//			return null;
//		}
//
//		private MotionIndex interpolateLinear(MotionPair mp, MotionIndexA mi, float frame, MotionIndex m) {
//			if (mp == null) {
//				return null;
//			} else if (mp.m1 == -1) {
//				System.arraycopy(mi.location, mp.m0 * 3, m.location, 0, 3);
//				System.arraycopy(mi.rotation, mp.m0 * 4, m.rotation, 0, 4);
//				return m;
//			} else {
//				int dif = mi.frame_no[mp.m1] - mi.frame_no[mp.m0];
//				float a0 = frame - mi.frame_no[mp.m0];
//				float ratio = a0 / dif;
//
//				if (mi.interp_x == null || mi.interp_x[mp.m0 * 4] == -1) { // calcurated in preCalcIK
//					float t = ratio;
//					m.location[0] = mi.location[mp.m0 * 3 + 0] + (mi.location[mp.m1 * 3 + 0] - mi.location[mp.m0 * 3 + 0]) * t;
//					m.location[1] = mi.location[mp.m0 * 3 + 1] + (mi.location[mp.m1 * 3 + 1] - mi.location[mp.m0 * 3 + 1]) * t;
//					m.location[2] = mi.location[mp.m0 * 3 + 2] + (mi.location[mp.m1 * 3 + 2] - mi.location[mp.m0 * 3 + 2]) * t;
//					slerp(m.rotation, mi.rotation, mi.rotation, mp.m0 * 4, mp.m1 * 4, t);
//				} else {
//					double t = bazier(mi.interp_x, mp.m0 * 4, 1, ratio);
//					m.location[0] = (float) (mi.location[mp.m0 * 3 + 0] + (mi.location[mp.m1 * 3 + 0] - mi.location[mp.m0 * 3 + 0]) * t);
//					t = bazier(mi.interp_y, mp.m0 * 4, 1, ratio);
//					m.location[1] = (float) (mi.location[mp.m0 * 3 + 1] + (mi.location[mp.m1 * 3 + 1] - mi.location[mp.m0 * 3 + 1]) * t);
//					t = bazier(mi.interp_z, mp.m0 * 4, 1, ratio);
//					m.location[2] = (float) (mi.location[mp.m0 * 3 + 2] + (mi.location[mp.m1 * 3 + 2] - mi.location[mp.m0 * 3 + 2]) * t);
//
//					slerp(m.rotation, mi.rotation, mi.rotation, mp.m0 * 4, mp.m1 * 4, bazier(mi.interp_a, mp.m0 * 4, 1, ratio));
//				}
//
//				return m;
//			}
//		}

	}

}

