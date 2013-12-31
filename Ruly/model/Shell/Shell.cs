using System.Collections.Generic;
//using Sce.PlayStation.Core;
//using Sce.PlayStation.Core.Graphics;
using System.Linq;
using Android.Util;
using Java.Nio;

using Ruly.viewmodel;
using Android.Opengl;
using System;

namespace Ruly.model
{
	public class RenderSet {
		public string shader;
		public string target;
		public RenderSet(string s, string t) {
			shader = s;
			target = t;
		}
	}

	public class Shell
	{
		public ShellSurface Surface {
			get;
			private set;
		}

		public Dictionary<string, ShellMotion> Motions {
			get;
			private set;
		}

		public string CurrentMotion {
			get;
			private set;
		}

		public List<RenderSet> RenderSets {
			get;
			private set;
		}

		public Shell()
		{
			Surface = null;
			RenderSets = new List<RenderSet> ();
			Motions = new Dictionary<string, ShellMotion> ();
		}

		public void LoadVMD (string root, string dir, string name)
		{
			RenderSets.Clear ();
			RenderSets.Add(new RenderSet("builtin:default", "screen"));
			RenderSets.Add(new RenderSet("builtin:default_alpha", "screen"));

			string path = root + dir + name;
			Motions[path] = new VMD (path);
			CurrentMotion = path;
			Surface.Animation = true;
		}


		public void LoadPMD(string root, string dir, string name)
		{
			RenderSets.Add(new RenderSet("builtin:nomotion", "screen"));
			RenderSets.Add(new RenderSet("builtin:nomotion_alpha", "screen"));

			string path = root + dir + name;
			var surface = new PMD (path);
			surface.SetupShellSurface ();
			Surface = surface;

			AddTextures (root, dir);

			// Load ends
			Surface.Animation = false;
			Surface.Loaded = true;

			Log.Debug ("Shell", "PMD load ends.");
		}

		private void AddTextures(string root, string dir)
		{
			// create texture entry for future texture read
			foreach (var m in Surface.RenderLists) {
				if (m.material.texture != null) {
					m.material.texture = TextureFile.SearchTextureFilePath (root, dir, m.material.texture);
					if (m.material.texture != null) {
						TextureFile.AddTexture (m.material.texture);
					}
				}
			}

			for (int i = 0; i < Surface.toon_name.Length; i++) {
				Surface.toon_name [i] = TextureFile.SearchTextureFilePath (root, dir, Surface.toon_name [i]);
				if (Surface.toon_name [i] != null) {
					TextureFile.AddTexture (Surface.toon_name [i]);
				}
			}
		}


		public void MoveBoneAtFrame(float i) {
			var ba = Surface.RenderBones;
			if(ba != null) {
				int max = ba.Count;
//				Log.Debug ("Ruly.Shell", "MoveBoneAtFrame " + i.ToString ());

				for (int r = 0; r < max; r++) {
					var b = ba[r];
					setBoneMatrix(b, i);
				}

				if(Surface.IK != null) {
					ccdIK();
				}

				for (int r = 0; r < max; r++) {
					var b = ba[r];
					updateBoneMatrix(b);
				}

				for (int r = 0; r < max; r++) {
					var b = ba[r];
					Matrix.TranslateM(b.matrix, 0, -b.bone.head_pos[0], -b.bone.head_pos[1], -b.bone.head_pos[2]);
					b.updated = false;
				}
			}
		}

		private void setBoneMatrix(RenderBone b, float idx) {
			var m = Motions [CurrentMotion].BoneMotionAt (b, idx);

			if (m != null) {
				b.quaternion[0] = m.rotation[0];
				b.quaternion[1] = m.rotation[1];
				b.quaternion[2] = m.rotation[2];
				b.quaternion[3] = m.rotation[3];
				Quaternion.toMatrix(b.matrix_current, m.rotation);

				if (b.bone.parent == -1) {
					b.matrix_current[12] = m.location[0] + b.bone.head_pos[0];
					b.matrix_current[13] = m.location[1] + b.bone.head_pos[1];
					b.matrix_current[14] = m.location[2] + b.bone.head_pos[2];
				} else {
					var p = Surface.RenderBones[b.bone.parent];
					b.matrix_current[12] = m.location[0] + (b.bone.head_pos[0] - p.bone.head_pos[0]);
					b.matrix_current[13] = m.location[1] + (b.bone.head_pos[1] - p.bone.head_pos[1]);
					b.matrix_current[14] = m.location[2] + (b.bone.head_pos[2] - p.bone.head_pos[2]);
				}
			} else {
				// no VMD info so assume that no rotation and translation are specified
				Matrix.SetIdentityM(b.matrix_current, 0);
				Quaternion.setIndentity(b.quaternion);
				if (b.bone.parent == -1) {
					Matrix.TranslateM(b.matrix_current, 0, b.bone.head_pos[0], b.bone.head_pos[1], b.bone.head_pos[2]);
				} else {
					var p = Surface.RenderBones[b.bone.parent];
					Matrix.TranslateM(b.matrix_current, 0,  b.bone.head_pos[0],  b.bone.head_pos[1],  b.bone.head_pos[2]);
					Matrix.TranslateM(b.matrix_current, 0, -p.bone.head_pos[0], -p.bone.head_pos[1], -p.bone.head_pos[2]);
				}
			}
		}

		private void updateBoneMatrix(RenderBone b) {
			if (b.updated == false) {
				if (b.bone.parent != -1) {
					var p = Surface.RenderBones[b.bone.parent];
					updateBoneMatrix(p);
					Matrix.MultiplyMM(b.matrix, 0, p.matrix, 0, b.matrix_current, 0);
				} else {
					for (int i = 0; i < 16; i++) {
						b.matrix[i] = b.matrix_current[i];
					}
				}
				b.updated = true;
			}
		}

		private void ccdIK() {
			foreach (var ik in Surface.IK) {
				ccdIK1(ik);
			}
		}

		private void ccdIK1(IK ik) {
			var effecterVecs = new float[4];
			var targetVecs = new float[4];
			var targetInvs = new float[4];
			var effecterInvs = new float[4];
			var axis = new float[3];
			var mQuatworks = new double[4];
			var mMatworks = new float[16];

			var effecter = Surface.RenderBones[ik.ik_bone_index];
			var target = Surface.RenderBones[ik.ik_target_bone_index];

			getCurrentPosition(effecterVecs, effecter);

			for (int i = 0; i < ik.iterations; i++) {
				for (int j = 0; j < ik.ik_chain_length; j++) {
					var b = Surface.RenderBones[ik.ik_child_bone_index[j]];

					clearUpdateFlags(b, target);
					getCurrentPosition(targetVecs, target);

					if (b.bone.is_leg) {
						if (i == 0) {
							var ba = Surface.RenderBones[ik.ik_child_bone_index[ik.ik_chain_length - 1]];
							getCurrentPosition(targetInvs, b);
							getCurrentPosition(effecterInvs, ba);

							double eff_len = Matrix.Length(effecterVecs[0] - effecterInvs[0], effecterVecs[1] - effecterInvs[1], effecterVecs[2] - effecterInvs[2]);
							double b_len = Matrix.Length(targetInvs[0] - effecterInvs[0], targetInvs[1] - effecterInvs[1], targetInvs[2] - effecterInvs[2]);
							double t_len = Matrix.Length(targetVecs[0] - targetInvs[0], targetVecs[1] - targetInvs[1], targetVecs[2] - targetInvs[2]);

							double angle1 = Math.Acos((eff_len * eff_len - b_len * b_len - t_len * t_len) / (2 * b_len * t_len));
							if (!double.IsNaN(angle1)) {
								axis[0] = -1;
								axis[1] = axis[2] = 0;
								Quaternion.createFromAngleAxis(mQuatworks, angle1, axis);
								Quaternion.mul(b.quaternion, b.quaternion, mQuatworks);
								Quaternion.toMatrixPreserveTranslate(b.matrix_current, b.quaternion);
							}
						}
						continue;
					}

					if (Matrix.Length(targetVecs[0] - effecterVecs[0], targetVecs[1] - effecterVecs[1], targetVecs[2] - effecterVecs[2]) < 0.001f) {
						// clear all
						foreach (var c in Surface.RenderBones) {
							c.updated = false;
						}
						return;
					}

					float[] current = getCurrentMatrix(b);
					Vector.invertM(mMatworks, 0, current, 0);
					Matrix.MultiplyMV(effecterInvs, 0, mMatworks, 0, effecterVecs, 0);
					Matrix.MultiplyMV(targetInvs, 0, mMatworks, 0, targetVecs, 0);

					// calculate rotation angle/axis
					Vector.normalize(effecterInvs);
					Vector.normalize(targetInvs);
					double angle2 = Math.Acos(Vector.dot(effecterInvs, targetInvs));
					angle2 *= ik.control_weight;

					if (!double.IsNaN(angle2)) {
						Vector.cross(axis, targetInvs, effecterInvs);
						Vector.normalize(axis);

						if (!double.IsNaN(axis[0]) && !double.IsNaN(axis[1]) && !double.IsNaN(axis[2])) {
							Quaternion.createFromAngleAxis(mQuatworks, angle2, axis);
							Quaternion.mul(b.quaternion, b.quaternion, mQuatworks);
							Quaternion.toMatrixPreserveTranslate(b.matrix_current, b.quaternion);
						}
					}
				}
			}
			// clear all
			foreach (var b in Surface.RenderBones) {
				b.updated = false;
			}
		}

		private void clearUpdateFlags(RenderBone root, RenderBone b) {
			while (root != b) {
				b.updated = false;
				if (b.bone.parent != -1) {
					b = Surface.RenderBones[b.bone.parent];
				} else {
					return;
				}
			}
			root.updated = false;
		}

		private void getCurrentPosition(float[] v, RenderBone b) {
			float[] current = getCurrentMatrix(b);
			Array.Copy(current, 12, v, 0, 3);
			v[3] = 1;
		}

		private float[] getCurrentMatrix(RenderBone b) {
			updateBoneMatrix(b);
			return b.matrix;
		}


	}
}

