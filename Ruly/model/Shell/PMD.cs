using System;
using System.IO;
using System.Text;

using Android.Util;
using Android.App;
using Java.Nio;
using Ruly.viewmodel;
using System.Collections.Generic;

namespace Ruly.model
{
	public class PMD : ShellSurface
	{
		private bool 		_is_pmd = false;
		public  bool  		 is_pmd		{ get { return _is_pmd; } }
		
		public string Description {
			get;
			set;
		}

		public float[]		Vertex;
		public float[]		Normal;
		public float[]		Uv;
		public short[]		Weight;
		public short[]		Index;
		public Material[]	Material;


		public Bone[]		Bone;
		private Face[]		Face;
		
		private int			m_skin_disp_num;
		private int			m_bone_disp_name_num;
		
		public PMD (string path)
		{
			string dir = Path.GetDirectoryName(path);
			Log.Debug ("PMD", "base dir: " + dir);
			using (var fs = File.OpenRead(path)) {
				using (var br = new BinaryReader (fs)) {
					init(br, dir + "/");
				}
			}
		}
		
		private void init (BinaryReader br, string path)
		{
			parseHeader(br);
			if(is_pmd) {

				// default toon filename
				toon_name = new string[11];
				toon_name[0] = "toon0.bmp";
				for(int i = 1; i < 10; i++) {
					toon_name[i] = "toon0" + i.ToString() + ".bmp";
				}
				toon_name[10] = "toon10.bmp";
				
				parseVertexList			( br );
				parseIndexList			( br );
				parseMaterialList		( br, path );
				parseBoneList			( br );
				parseIKList				( br );
				parseFaceList			( br );
				parseSkinDisp			( br );
				parseBoneDispName		( br );
				parseBoneDisp			( br );

				if(br.BaseStream.Position != br.BaseStream.Length) {
					Log.Debug ("PMD", "has more data...");
					Log.Debug ("PMD", "position = " + br.BaseStream.Position.ToString() + ", length = " + br.BaseStream.Length.ToString());
					parseEnglish		( br );
					parseToonFileName	( br, path );
					/*
					parseRigidBody();
					parseJoint();
					*/
				}
			}
		}

		public override void SetupShellSurface ()
		{
			// create buffers for render
			ByteBuffer buf = ByteBuffer.AllocateDirect (Vertex.Length * sizeof(float));
			buf.Order (ByteOrder.NativeOrder ());
			base.VertexBuffer = buf.AsFloatBuffer() as FloatBuffer;
			base.VertexBuffer.Put (Vertex);
			base.VertexBuffer.Position (0);

			buf = ByteBuffer.AllocateDirect (Normal.Length * sizeof(float));
			buf.Order (ByteOrder.NativeOrder ());
			base.NormalBuffer = buf.AsFloatBuffer() as Java.Nio.FloatBuffer;
			base.NormalBuffer.Put (Normal);
			base.NormalBuffer.Position (0);

			buf = ByteBuffer.AllocateDirect (Uv.Length * sizeof(float));
			buf.Order (ByteOrder.NativeOrder ());
			base.UvBuffer = buf.AsFloatBuffer() as Java.Nio.FloatBuffer;
			base.UvBuffer.Put (Uv);
			base.UvBuffer.Position (0);

			buf = ByteBuffer.AllocateDirect (Weight.Length * sizeof(short));
			buf.Order (ByteOrder.NativeOrder ());
			base.WeightBuffer = buf.AsShortBuffer() as Java.Nio.ShortBuffer;
			base.WeightBuffer.Put (Weight);
			base.WeightBuffer.Position (0);

			buf = ByteBuffer.AllocateDirect (Index.Length * sizeof(short));
			buf.Order (ByteOrder.NativeOrder ());
			base.IndexBuffer = buf.AsShortBuffer() as Java.Nio.ShortBuffer;
			base.IndexBuffer.Put (Index);
			base.IndexBuffer.Position (0);

			CreateRenderList ();

			// clearnup buffers
			Vertex = null;
			Normal = null;
			Uv = null;
			Weight = null;
			Index = null;
		}

		private void CreateRenderList()
		{
			var rename_pool = new Dictionary<Dictionary<int, int>, ByteBuffer>();

			foreach (var i in Material) {
				reconstructMaterial1 (i, 0, rename_pool, 48);	//ad-hock
//				RenderList.Add (i);
			}
			Material = null;
		}


		private void reconstructMaterial1(Material mat, int offset, Dictionary<Dictionary<int, int>, ByteBuffer> rename_pool, int max_bone) {
			var rename = new Dictionary<int, int>();
			int acc = 0;
			for (int j = offset; j < mat.face_vert_count; j += 3) {
				int acc_prev = acc;
				acc = renameBone1(rename, mat.face_vert_offset + j + 0, acc);
				acc = renameBone1(rename, mat.face_vert_offset + j + 1, acc);
				acc = renameBone1(rename, mat.face_vert_offset + j + 2, acc);
				if (acc > max_bone) {
					RenderList mat_new = buildNewMaterial(mat, offset, j, rename, rename_pool, acc_prev);
					RenderLists.Add(mat_new);
					reconstructMaterial1(mat, j, rename_pool, max_bone);
					return;
				}
			}
			RenderList mat_new2 = buildNewMaterial(mat, offset, mat.face_vert_count, rename, rename_pool, max_bone);
			RenderLists.Add(mat_new2);
		}

		private int renameBone1(Dictionary<int, int> rename, int veridx, int acc) {
			int pos = (0x0000ffff & Index[veridx]);
			short bone_num_0 = Weight[pos * 3 + 0];
			short bone_num_1 = Weight[pos * 3 + 1];

			if (!rename.ContainsKey (bone_num_0)) {
				rename [bone_num_0] = acc++;
			}

			if (!rename.ContainsKey (bone_num_1)) {
				rename [bone_num_1] = acc++;
			}

			return acc;
		}


		private RenderList buildNewMaterial(Material mat_orig, int offset, int count, Dictionary<int, int> rename, Dictionary<Dictionary<int, int>, ByteBuffer> rename_pool, int max_bone) {
			RenderList mat = new RenderList();
			mat.material = mat_orig;
			mat.face_vert_offset = mat_orig.face_vert_offset + offset;
			mat.face_vert_count  = count - offset;
			mat.bone_num = rename.Count;

			// find unoverwrapped hash
			foreach(var pool in rename_pool) {
				var map = pool.Key;
				var bb = pool.Value;

				// check mapped
				foreach(var entry in rename) {
					if(map.ContainsKey(entry.Key)) {
						bb = null;
					}
				}

				// find free byte buffer
				if(bb != null) {
					rename_pool.Remove(map);
					mat.weight = bb;
					buildBoneRenamedWeightBuffers(mat, rename, max_bone);

					foreach (var i in rename) {
						map [i.Key] = i.Value;
					}
//					map.putAll(rename);
					rename_pool[map] = bb;
					Log.Debug("PMD", "Reuse buffer");
					return mat;
				}
			}

			// allocate new buffer
			Log.Debug("PMD", "Allocate new buffer");
			allocateWeightBuffer(mat, rename);
			buildBoneRenamedWeightBuffers(mat, rename, max_bone);

			var new_map = new Dictionary<int, int>(rename);
			rename_pool[new_map] = mat.weight;

			Log.Debug("PMD", "rename Bone for Material #" + mat.material.face_vert_offset + ", bones " + offset.ToString());
			foreach (var b in rename) {
				Log.Debug("PMD", String.Format("ID {0}: bone {1}", b.Value, b.Key));
			}
			return mat;
		}

		private void allocateWeightBuffer(RenderList mat, Dictionary<int, int> rename) {
			ByteBuffer rbb = ByteBuffer.AllocateDirect(Weight.Length);
			rbb.Order(ByteOrder.NativeOrder());
			mat.weight = rbb;
		}

		private void buildBoneRenamedWeightBuffers(RenderList mat, Dictionary<int, int> rename, int max_bone) {
			buildBoneRenameInvMap(mat, rename, max_bone);

			int[] map = buildBoneRenameMap(mat, rename, max_bone);

			short[] weight = new short[3];
			for (int i = mat.face_vert_offset; i < mat.face_vert_offset + mat.face_vert_count; i++) {
				int pos = (0x0000ffff & Index[i]);
				weight [0] = Weight [pos * 3 + 0];
				weight [1] = Weight [pos * 3 + 1];
				weight [2] = Weight [pos * 3 + 2];

				mat.weight.Position(pos * 3);
				mat.weight.Put((sbyte) map[weight[0]]);
				mat.weight.Put((sbyte) map[weight[1]]);
				mat.weight.Put((sbyte) weight[2]);
			}

			mat.weight.Position(0);
		}

		private int[] buildBoneRenameMap(RenderList mat, Dictionary<int, int> rename, int max_bone) {
			int[] rename_map = new int[Bone.Length];
			for (int i = 0; i < Bone.Length; i++) {
				rename_map[i] = 0; // initialize
			}
			foreach (var b in rename) {
				if (b.Value < max_bone) {
					rename_map[b.Key] = b.Value;
				}
			}

			return rename_map;
		}

		private void buildBoneRenameInvMap(RenderList mat, Dictionary<int, int> rename, int max_bone) {
			mat.bone_inv_map = new int[max_bone];
			for (int i = 0; i < max_bone; i++) {
				mat.bone_inv_map[i] = -1; // initialize
			}
			foreach (var b in rename) {
				if (b.Value < max_bone) {
					mat.bone_inv_map[b.Value] = b.Key;
				}
			}
		}



						
		private void parseHeader(BinaryReader br) {
			// Magic
			string s = new string(br.ReadChars (3));
			Log.Debug("PMD", "MAGIC: " + s);
			if (s == "Pmd") {
				_is_pmd = true;
			}

			// Version
			float f = br.ReadSingle();
			Log.Debug("PMD", "VERSION: " + f.ToString());

			// Model Name
			ModelName = Util.ReadString(br, 20);
			Log.Debug("PMD", "MODEL NAME: ");
			Log.Debug("PMD", ModelName);

			// description
			Description = Util.ReadString (br, 256);
			Log.Debug("PMD", "DESCRIPTION: ");
			Log.Debug("PMD", Description);
		}
		
		private void parseVertexList(BinaryReader br) {
			// the number of Vertexes
			int num = br.ReadInt32 ();
			Log.Debug("PMD", "VERTEX: " + num.ToString());
			if (num > 0) {
				Vertex = new float[num * 3];
				Normal = new float[num * 3];
				Uv = new float[num * 2];
				Weight = new short[num * 3];
				for(int i = 0; i < num; i++) {
					Vertex [i * 3 + 0] = br.ReadSingle ();
					Vertex [i * 3 + 1] = br.ReadSingle ();
					Vertex [i * 3 + 2] = br.ReadSingle ();

					Normal [i * 3 + 0] = br.ReadSingle ();
					Normal [i * 3 + 1] = br.ReadSingle ();
					Normal [i * 3 + 2] = br.ReadSingle ();

					Uv [i * 2 + 0] = br.ReadSingle ();
					Uv [i * 2 + 1] = br.ReadSingle ();

					Weight [i * 3 + 0] = br.ReadInt16 ();
					Weight [i * 3 + 1] = br.ReadInt16 ();
					Weight [i * 3 + 2] = (short)br.ReadByte ();

					br.ReadBytes (1);	// edge flag
				}
			} else {
				Vertex = null;
			}
		}
		
		private void parseIndexList(BinaryReader br) {
			// the number of Vertexes
			int num = br.ReadInt32();
			Log.Debug("PMD", "INDEX: " + num.ToString());
			if(num > 0) {
				Index = new short[num];
				for(int i = 0; i < num; i++) {
					Index [i] = br.ReadInt16 ();
				}
			} else {
				Index = null;
			}
		}

		private void parseMaterialList(BinaryReader br, String path) {
			// the number of Vertexes
			int num = br.ReadInt32 ();
			Log.Debug("PMD", "MATERIAL: " + num.ToString());
			if (num > 0) {
				int acc = 0;
				Material = new Material[num];
				for (int i = 0; i < num; i++) {
					Material m = new Material();
										
					m.diffuse_color		= Util.ReadFloats(br, 4);
					m.power				= br.ReadSingle();
					m.specular_color	= Util.ReadFloats(br, 3);
					m.emmisive_color	= Util.ReadFloats(br, 3);
					
					m.toon_index		= (byte)(br.ReadByte() + 1);
					m.edge_flag			= br.ReadByte();
					m.face_vert_count	= br.ReadInt32();
					m.texture			= Util.ReadString(br, 20);
					
					m.face_vert_offset	= acc;
					acc = acc + m.face_vert_count;
					
					Material[i] = m;
				}
				Log.Debug("PMD", "CHECKSUM IN MATERIAL: " + acc.ToString());
			} else {
				Material = null;
			}
		}
		
		private void parseBoneList(BinaryReader br) {
			// the number of Vertexes
			short num = br.ReadInt16();
			Log.Debug("PMD", "BONE: " + num.ToString());
			if (num > 0) {
				Bone = new Bone[num];
				for (int i = 0; i < num; i++) {
					Bone b = new Bone();

					b.name = Util.ReadString(br, 20);
					b.parent = br.ReadInt16();
					b.tail = br.ReadInt16();
					b.type = br.ReadByte();
					b.ik = br.ReadInt16();

					b.head_pos = Util.ReadFloats(br, 3);
					b.is_leg = b.name.Contains("ひざ");
				
					if (b.tail != -1) {
						Bone[i] = b;
					}
				}
			}
		}
		
		private void parseIKList(BinaryReader br) {
			// the number of Vertexes
			short num = br.ReadInt16();
			Log.Debug("PMD", "IK: " + num.ToString());
			if (num > 0) {
				IK = new IK[num];
				for (int i = 0; i < num; i++) {
					IK ik = new IK();

					ik.ik_bone_index = br.ReadInt16();
					ik.ik_target_bone_index = br.ReadInt16();
					ik.ik_chain_length = br.ReadByte();
					ik.iterations = br.ReadInt16();
					ik.control_weight = br.ReadSingle();

					ik.ik_child_bone_index = new short[ik.ik_chain_length];
					for (int j = 0; j < ik.ik_chain_length; j++) {
						ik.ik_child_bone_index[j] = br.ReadInt16();
					}

					IK[i] = ik;
				}
			} else {
				IK = null;
			}
		}
		
		private void parseFaceList(BinaryReader br)
		{
			short num = br.ReadInt16();
			int acc = 0;
			Log.Debug("PMD", "Face: " + num.ToString());
			if (num > 0) {
				Face = new Face[num];
				for (int i = 0; i < num; i++) {
					Face face = new Face();
					face.name				= Util.ReadString(br, 20);
					face.face_vert_count	= br.ReadInt32();
					face.face_type			= br.ReadByte();
					face.face_vert_index	= new int[face.face_vert_count];
					face.face_vert_offset	= new float[face.face_vert_count][];
					
					for (int j = 0; j < face.face_vert_count; j++) {
						face.face_vert_index[j]		= br.ReadInt32();
						face.face_vert_offset[j]	= Util.ReadFloats(br, 3);
					}
					
					acc += face.face_vert_count;					
					Face[i] = face;
				}
				Log.Debug("PMD", "Total Face Vert: " + acc.ToString());
			} else {
				Face = null;
			}
		}
		
		private void parseSkinDisp(BinaryReader br) {
			byte num = br.ReadByte();
			m_skin_disp_num = num;
			Log.Debug("PMD", "SkinDisp: " + num.ToString());
			if (num > 0) {
				br.ReadBytes (2 * num);
			}
		}
		
		private void parseBoneDispName(BinaryReader br) {
			byte num = br.ReadByte();
			m_bone_disp_name_num = num;
			Log.Debug("PMD", "BoneDispName: " + num.ToString());
			if (num > 0) {
				br.ReadBytes (50 * num);
			}
		}
		
		private void parseBoneDisp(BinaryReader br) {
			int num = br.ReadInt32();
			Log.Debug("PMD", "BoneDisp: " + num.ToString());
			if (num > 0) {
				br.ReadBytes (3 * num);
			}
		}
		
		private void parseEnglish(BinaryReader br)
		{
			byte has_english = br.ReadByte();
			Log.Debug("PMD", "HasEnglishName: " + has_english.ToString());
			if (has_english == 1) {
				parseEnglishName(br);
				parseEnglishBoneList(br);
				parseEnglishSkinList(br);
				parseEnglishBoneDispName(br);
			}
		}
		
		private void parseEnglishName(BinaryReader br) {
			br.ReadBytes(20 + 256);
		}
		
		private void parseEnglishBoneList(BinaryReader br) {
			int num = Bone.Length;
			Log.Debug("PMD", "EnglishBoneName: " + num.ToString());
			if(num > 0) {
				br.ReadBytes(20 * num);
				/*
				mEnglishBoneName = new ArrayList<String>(num);
				for (int i = 0; i < num; i++) {
					String str = getString(20);
					mEnglishBoneName.add(i, str);
				}
				*/
			}
		}

		private void parseEnglishSkinList(BinaryReader br) {
			int num = m_skin_disp_num;
			Log.Debug("PMD", "EnglishSkinName: " + num.ToString());
			if(num > 0) {
				br.ReadBytes(20 * num);
				/*
				mEnglishSkinName = new ArrayList<String>(num);
				for (int i = 0; i < num; i++) {
					String str = getString(20);
					mEnglishSkinName.add(i, str);
				}
				*/
			}
		}
		
		private void parseEnglishBoneDispName(BinaryReader br) {
			int num = m_bone_disp_name_num;
			if(num > 0) {
				br.ReadBytes(50 * num);
				/*
				mEnglishBoneDispName = new ArrayList<String>(num);
				for (int i = 0; i < num; i++) {
					String str = getString(50);
					mEnglishBoneDispName.add(i, str);
				}
				*/
			}
		}
		

		private void parseToonFileName(BinaryReader br, string path)
		{
			for (int i = 1; i < 11; i++) {
				toon_name[i] = Util.ReadString(br, 100);
			}
		}
	}
}

