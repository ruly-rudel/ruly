using System;
using System.IO;
using System.Text;

using Android.Util;
using Android.App;

namespace Ruly.model
{
	public class PMD : ShellSurface
	{
		private bool 		_is_pmd = false;
		public  bool  		 is_pmd		{ get { return _is_pmd; } }
		
		public string		model_name	{ get; set; }

		public float[]		Vertex;
		public float[]		Normal;
		public float[]		Uv;
		public short[]		Weight;
		public short[]		Index;

		private IK[]		m_IK;
		private Face[]		m_face;
		
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
			model_name = Util.ReadString(br, 20);
			Log.Debug("PMD", "MODEL NAME: ");
			Log.Debug("PMD", model_name);

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
					
					material.Add(m);
				}
				Log.Debug("PMD", "CHECKSUM IN MATERIAL: " + acc.ToString());
			} else {
				material = null;
			}
		}
		
		private void parseBoneList(BinaryReader br) {
			// the number of Vertexes
			short num = br.ReadInt16();
			Log.Debug("PMD", "BONE: " + num.ToString());
			if (num > 0) {
				bone = new Bone[num];
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
						bone[i] = b;
					}
				}
			}
		}
		
		private void parseIKList(BinaryReader br) {
			// the number of Vertexes
			short num = br.ReadInt16();
			Log.Debug("PMD", "IK: " + num.ToString());
			if (num > 0) {
				m_IK = new IK[num];
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

					m_IK[i] = ik;
				}
			} else {
				m_IK = null;
			}
		}
		
		private void parseFaceList(BinaryReader br)
		{
			short num = br.ReadInt16();
			int acc = 0;
			Log.Debug("PMD", "Face: " + num.ToString());
			if (num > 0) {
				m_face = new Face[num];
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
					m_face[i] = face;
				}
				Log.Debug("PMD", "Total Face Vert: " + acc.ToString());
			} else {
				m_face = null;
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
			int num = bone.Length;
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

