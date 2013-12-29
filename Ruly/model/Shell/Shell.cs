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

	public class Material {
		public float[]		diffuse_color;
		public float 		power;
		public float[]		specular_color;
		public float[]		emmisive_color;
		public byte			toon_index;
		public byte			edge_flag;
		public int			face_vert_count;
		public string 		texture;
		
		public int 			face_vert_offset;
	}

	public class RenderList {
		public Material		material;

		public int			face_vert_offset;
		public int			face_vert_count;
		public int			bone_num;
		public ByteBuffer	weight;
		public int[]		bone_inv_map;
	}
	
	public class Bone {
		public string		name;
		public short		parent;
		public short		tail;
		public byte			type;
		public short		ik;
		public float[]		head_pos;
		public bool			is_leg;
	}

	public class RenderBone {
		public Bone			bone;
		public float[]		matrix;
		public float[]		matrix_current;
		public double[]		quaternion;
		public bool updated;
	}
	
	public class IK {
		public int			ik_bone_index;
		public int			ik_target_bone_index;
		public byte			ik_chain_length;
		public int			iterations;
		public float		control_weight;
		public short[]		ik_child_bone_index;
	}
	
	public class Face {
		public string		name;
		public int			face_vert_count;
		public byte			face_type;
		public int[]		face_vert_index;
		public float[][]	face_vert_offset;
	}

	public class TexInfo {
		public int		tex;
		public bool		has_alpha;
		public bool		needs_alpha_test;
	}
	
	public abstract class ShellSurface
	{
		public bool Loaded {
			get;
			set;
		}

		public bool Animation {
			get;
			set;
		}


		public string						ModelName	{ get; set; }
		public List<RenderList>				RenderLists	{ set; get; }
		public List<RenderBone>				RenderBones { set; get; }
		public IK[]							IK;
		public string[]						toon_name;

		public FloatBuffer VertexBuffer;

		public FloatBuffer NormalBuffer;

		public FloatBuffer UvBuffer;

		public ShortBuffer WeightBuffer;

		public ShortBuffer IndexBuffer;

				
		public ShellSurface ()
		{
			RenderLists = new List<RenderList>();
			RenderBones = new List<RenderBone> ();
			Loaded = false;
		}

		public abstract void SetupShellSurface ();
	}

	public class BoneMotion
	{
		public int frame_no;
		public float[] location;
		public float[] rotation;
		public byte[] interp;
	}

	public class Morphing
	{
		public int frame_no;
		public float weight;
	}

	public class ShellMotion
	{
		public Dictionary<string, List<BoneMotion>> Bone {
			get;
			set;
		}

		public Dictionary<string, List<Morphing>> Morph {
			get;
			set;
		}

		public int max_frame {
			get;
			protected set;
		}

		public BoneMotion BoneMotionAt(float time)
		{
			throw new NotImplementedException ();
//			return Bone.First().Value[0];
		}
	}

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


		public void setBonePosByVMDFrame(float i) {
			var ba = Surface.RenderBones;
			if(ba != null) {
				int max = ba.Count;

				for (int r = 0; r < max; r++) {
					var b = ba[r];
					setBoneMatrix(b, i);
				}

//				if(Surface.IK != null && mMotion.getIKMotion() == null) {
//					ccdIK();
//				}

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
			var m = Motions [CurrentMotion].BoneMotionAt (idx);
//			MotionPair mp = mMotion.findMotion(b, idx, mMpWork);
//			MotionIndex m = mMotion.interpolateLinear(mp, b.motion, idx, mMwork);
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
//			var effecter = Surface.RenderBones[ik.ik_bone_index];
//			var target = Surface.RenderBones[ik.ik_target_bone_index];
//
//			getCurrentPosition(effecterVecs, effecter);
//
//			for (int i = 0; i < ik.iterations; i++) {
//				for (int j = 0; j < ik.ik_chain_length; j++) {
//					var b = Surface.RenderBones[ik.ik_child_bone_index[j]];
//
//					clearUpdateFlags(b, target);
//					getCurrentPosition(targetVecs, target);
//
//					if (b.is_leg) {
//						if (i == 0) {
//							var ba = Surface.RenderBones[ik.ik_child_bone_index[ik.ik_chain_length - 1]];
//							getCurrentPosition(targetInvs, b);
//							getCurrentPosition(effecterInvs, ba);
//
//							double eff_len = Matrix.Length(effecterVecs[0] - effecterInvs[0], effecterVecs[1] - effecterInvs[1], effecterVecs[2] - effecterInvs[2]);
//							double b_len = Matrix.Length(targetInvs[0] - effecterInvs[0], targetInvs[1] - effecterInvs[1], targetInvs[2] - effecterInvs[2]);
//							double t_len = Matrix.Length(targetVecs[0] - targetInvs[0], targetVecs[1] - targetInvs[1], targetVecs[2] - targetInvs[2]);
//
//							double angle = Math.Acos((eff_len * eff_len - b_len * b_len - t_len * t_len) / (2 * b_len * t_len));
//							if (!double.IsNaN(angle)) {
//								axis[0] = -1;
//								axis[1] = axis[2] = 0;
//								Quaternion.createFromAngleAxis(mQuatworks, angle, axis);
//								Quaternion.mul(b.quaternion, b.quaternion, mQuatworks);
//								Quaternion.toMatrixPreserveTranslate(b.matrix_current, b.quaternion);
//							}
//						}
//						continue;
//					}
//
//					if (Matrix.Length(targetVecs[0] - effecterVecs[0], targetVecs[1] - effecterVecs[1], targetVecs[2] - effecterVecs[2]) < 0.001f) {
//						// clear all
//						foreach (var c in Surface.RenderBones) {
//							c.updated = false;
//						}
//						return;
//					}
//
//					float[] current = getCurrentMatrix(b);
//					Vector.invertM(mMatworks, 0, current, 0);
//					Matrix.MultiplyMV(effecterInvs, 0, mMatworks, 0, effecterVecs, 0);
//					Matrix.MultiplyMV(targetInvs, 0, mMatworks, 0, targetVecs, 0);
//
//					// calculate rotation angle/axis
//					Vector.normalize(effecterInvs);
//					Vector.normalize(targetInvs);
//					double angle = Math.Acos(Vector.dot(effecterInvs, targetInvs));
//					angle *= ik.control_weight;
//
//					if (!double.IsNaN(angle)) {
//						Vector.cross(axis, targetInvs, effecterInvs);
//						Vector.normalize(axis);
//
//						// rotateM(mMatworks, 0, b.matrix_current, 0, degree, axis[0], axis[1], axis[2]);
//						// System.arraycopy(mMatworks, 0, b.matrix_current, 0, 16);
//						if (!double.IsNaN(axis[0]) && !double.IsNaN(axis[1]) && !double.IsNaN(axis[2])) {
//							Quaternion.createFromAngleAxis(mQuatworks, angle, axis);
//							Quaternion.mul(b.quaternion, b.quaternion, mQuatworks);
//							Quaternion.toMatrixPreserveTranslate(b.matrix_current, b.quaternion);
//						}
//					}
//				}
//			}
//			// clear all
//			foreach (var b in Surface.RenderBones) {
//				b.updated = false;
//			}
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
//			System.arraycopy(current, 12, v, 0, 3);
			Array.Copy(current, 12, v, 0, 3);
			v[3] = 1;
		}

		private float[] getCurrentMatrix(RenderBone b) {
			updateBoneMatrix(b);
			return b.matrix;
		}


	}
}

