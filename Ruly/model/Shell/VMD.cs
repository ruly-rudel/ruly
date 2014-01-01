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

		public override BoneMotion BoneMotionAt (RenderBone b, float frame)
		{
			if (Bone.ContainsKey (b.bone.name)) {
				var bones = Bone [b.bone.name];
				var m0 = from x in bones
					         where x.frame_no >= frame
				     select x;
				var m1 = from x in bones
					         where x.frame_no <= frame
				         select x;

				if (m0.Count () == 0) {
					if (m1.Count () == 0) { 
						return null;
					} else {
						return m1.Last ();
					}
				} else {
					var b0 = m0.First ();
					if (m1.Count () == 0) {
						return b0;
					} else {
						var b1 = m1.Last ();
						if (b0.Equals (b1)) {
							return b0;
						} else {	// interpolate
							int dif = b1.frame_no - b0.frame_no;
							float a0 = frame - b0.frame_no;
							float ratio = a0 / dif;

							var m = new BoneMotion ();
							m.location = new float[3];
							m.rotation = new float[4];
							double t = bazier(b0.interp, 0, 1, ratio);
							m.location[0] = (float) (b0.location[0] + (b1.location[0] - b0.location[0]) * t);
							t = bazier(b0.interp, 1, 1, ratio);
							m.location[1] = (float) (b0.location[1] + (b1.location[1] - b0.location[1]) * t);
							t = bazier(b0.interp, 2, 1, ratio);
							m.location[2] = (float) (b0.location[2] + (b1.location[2] - b0.location[2]) * t);

							slerp(m.rotation, b0.rotation, b1.rotation, 0, 0, bazier(b0.interp, 3, 1, ratio));

							return m;
						}
					}
				}
			} else {
				return null;
			}
		}

		#endregion

		private void slerp(float[] p, float[] q, float[] r, int m0, int m1, double t) {
			double qr = q[m0 + 0] * r[m1 + 0] + q[m0 + 1] * r[m1 + 1] + q[m0 + 2] * r[m1 + 2] + q[m0 + 3] * r[m1 + 3];
			double ss = 1.0 - qr * qr;

			if (qr < 0) {
				qr = -qr;

				double sp = Math.Sqrt(ss);
				double ph = Math.Acos(qr);
				double pt = ph * t;
				double t1 = Math.Sin(pt) / sp;
				double t0 = Math.Sin(ph - pt) / sp;

				if (double.IsNaN(t0) || double.IsNaN(t1)) {
					p[0] = q[m0 + 0];
					p[1] = q[m0 + 1];
					p[2] = q[m0 + 2];
					p[3] = q[m0 + 3];
				} else {
					p[0] = (float) (q[m0 + 0] * t0 - r[m1 + 0] * t1);
					p[1] = (float) (q[m0 + 1] * t0 - r[m1 + 1] * t1);
					p[2] = (float) (q[m0 + 2] * t0 - r[m1 + 2] * t1);
					p[3] = (float) (q[m0 + 3] * t0 - r[m1 + 3] * t1);
				}

			} else {
				double sp = Math.Sqrt(ss);
				double ph = Math.Acos(qr);
				double pt = ph * t;
				double t1 = Math.Sin(pt) / sp;
				double t0 = Math.Sin(ph - pt) / sp;

				if (double.IsNaN(t0) || double.IsNaN(t1)) {
					p[0] = q[m0 + 0];
					p[1] = q[m0 + 1];
					p[2] = q[m0 + 2];
					p[3] = q[m0 + 3];
				} else {
					p[0] = (float) (q[m0 + 0] * t0 + r[m1 + 0] * t1);
					p[1] = (float) (q[m0 + 1] * t0 + r[m1 + 1] * t1);
					p[2] = (float) (q[m0 + 2] * t0 + r[m1 + 2] * t1);
					p[3] = (float) (q[m0 + 3] * t0 + r[m1 + 3] * t1);
				}
			}
		}

		private double bazier(byte[] ip, int ofs, int size, float t) {
			double xa = ip[ofs] / 256;
			double xb = ip[size * 2 + ofs] / 256;
			double ya = ip[size + ofs] / 256;
			double yb = ip[size * 3 + ofs] / 256;

			double min = 0;
			double max = 1;

			double ct = t;
			while (true) {
				double x11 = xa * ct;
				double x12 = xa + (xb - xa) * ct;
				double x13 = xb + (1 - xb) * ct;

				double x21 = x11 + (x12 - x11) * ct;
				double x22 = x12 + (x13 - x12) * ct;

				double x3 = x21 + (x22 - x21) * ct;

				if (Math.Abs(x3 - t) < 0.0001) {
					double y11 = ya * ct;
					double y12 = ya + (yb - ya) * ct;
					double y13 = yb + (1 - yb) * ct;

					double y21 = y11 + (y12 - y11) * ct;
					double y22 = y12 + (y13 - y12) * ct;

					double y3 = y21 + (y22 - y21) * ct;

					return y3;
				} else if (x3 < t) {
					min = ct;
				} else {
					max = ct;
				}
				ct = min * 0.5 + max * 0.5;
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

