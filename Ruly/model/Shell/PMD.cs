using System;
using System.IO;
using System.Text;

//using Sce.PlayStation.Core;
//using Sce.PlayStation.Core.Graphics;
//using I18N.CJK;
using Android.Util;
using Android.App;

namespace Ruly.model
{
	public class PMD : ShellSurface
	{
		private bool 		_is_pmd = false;
		public  bool  		 is_pmd		{ get { return _is_pmd; } }
		
		public string		model_name	{ get; set; }
		public string		description { get; set; }

		public float[]		Vertex;
		public float[]		Normal;
		public float[]		Uv;
		public short[]		Index;

		private Bone[]		m_bone;
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
					init(br, fs, dir + "/");
				}
			}
		}
		
		private void init (BinaryReader br, Stream fs, string path)
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
					Log.Debug ("PMD", "position = " + fs.Position.ToString() + ", length = " + fs.Length.ToString());
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
			model_name = readString(br, 20);
			Log.Debug("PMD", "MODEL NAME: ");
			Log.Debug("PMD", model_name);

			// description
			description = readString (br, 256);
			Log.Debug("PMD", "DESCRIPTION: ");
			Log.Debug("PMD", description);
		}
		
		private void parseVertexList(BinaryReader br) {
			// the number of Vertexes
			int num = br.ReadInt32 ();
			Log.Debug("PMD", "VERTEX: " + num.ToString());
			if (num > 0) {
				Vertex = new float[num * 3];
				Normal = new float[num * 3];
				Uv = new float[num * 2];
				for(int i = 0; i < num; i++) {
					Vertex [i * 3 + 0] = br.ReadSingle ();
					Vertex [i * 3 + 1] = br.ReadSingle ();
					Vertex [i * 3 + 2] = br.ReadSingle ();

					Normal [i * 3 + 0] = br.ReadSingle ();
					Normal [i * 3 + 1] = br.ReadSingle ();
					Normal [i * 3 + 2] = br.ReadSingle ();

					Uv [i * 2 + 0] = br.ReadSingle ();
					Uv [i * 2 + 1] = br.ReadSingle ();

//					for(int j = 0; j < 8; j++) {
//						Vertex[i * 8 + j] = br.ReadSingle();
//					}
					br.ReadBytes (6);
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
										
					m.diffuse_color		= readFloats(br, 4);
					m.power				= br.ReadSingle();
					m.specular_color	= readFloats(br, 3);
					m.emmisive_color	= readFloats(br, 3);
					
					m.toon_index		= (byte)(br.ReadByte() + 1);
					m.edge_flag			= br.ReadByte();
					m.face_vert_count	= br.ReadInt32();
					m.texture			= readString(br, 20);
					
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
			Log.Debug("PMDParser", "BONE: " + num.ToString());
			if (num > 0) {
				m_bone = new Bone[num];
				for (int i = 0; i < num; i++) {
					Bone bone = new Bone();

					bone.name = readString(br, 20);
					bone.parent = br.ReadInt16();
					bone.tail = br.ReadInt16();
					bone.type = br.ReadByte();
					bone.ik = br.ReadInt16();

					bone.head_pos = readFloats(br, 3);
//					bone.is_leg = bone.name.contains("ひざ");
				
					if (bone.tail != -1) {
						m_bone[i] = bone;
					}
				}
			}
		}
		
		private void parseIKList(BinaryReader br) {
			// the number of Vertexes
			short num = br.ReadInt16();
			Log.Debug("PMDParser", "IK: " + num.ToString());
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
			Log.Debug("PMDParser", "Face: " + num.ToString());
			if (num > 0) {
				m_face = new Face[num];
				for (int i = 0; i < num; i++) {
					Face face = new Face();
					face.name				= readString(br, 20);
					face.face_vert_count	= br.ReadInt32();
					face.face_type			= br.ReadByte();
					face.face_vert_index	= new int[face.face_vert_count];
					face.face_vert_offset	= new float[face.face_vert_count][];
					
					for (int j = 0; j < face.face_vert_count; j++) {
						face.face_vert_index[j]		= br.ReadInt32();
						face.face_vert_offset[j]	= readFloats(br, 3);
					}
					
					acc += face.face_vert_count;					
					m_face[i] = face;
				}
				Log.Debug("PMDParser", "Total Face Vert: " + acc.ToString());
			} else {
				m_face = null;
			}
		}
		
		private void parseSkinDisp(BinaryReader br) {
			byte num = br.ReadByte();
			m_skin_disp_num = num;
			Log.Debug("PMDParser", "SkinDisp: " + num.ToString());
			if (num > 0) {
				br.ReadBytes (2 * num);
			}
		}
		
		private void parseBoneDispName(BinaryReader br) {
			byte num = br.ReadByte();
			m_bone_disp_name_num = num;
			Log.Debug("PMDParser", "BoneDispName: " + num.ToString());
			if (num > 0) {
				br.ReadBytes (50 * num);
			}
		}
		
		private void parseBoneDisp(BinaryReader br) {
			int num = br.ReadInt32();
			Log.Debug("PMDParser", "BoneDisp: " + num.ToString());
			if (num > 0) {
				br.ReadBytes (3 * num);
			}
		}
		
		private void parseEnglish(BinaryReader br)
		{
			byte has_english = br.ReadByte();
			Log.Debug("PMDParser", "HasEnglishName: " + has_english.ToString());
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
			int num = m_bone.Length;
			Log.Debug("PMDParser", "EnglishBoneName: " + num.ToString());
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
			Log.Debug("PMDParser", "EnglishSkinName: " + num.ToString());
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
				toon_name[i] = readString(br, 100);
			}
		}

		private string readString(BinaryReader br, int num)
		{
			byte [] buf = br.ReadBytes(num);
			for(int i = 0; i < num; i++) {
				if(buf[i] == 0) {
					if(i == 0) {
						return null;
					} else {
						char [] r = new char[i];
						for(int j = 0; j < i; j++) {
							r[j] = (char)buf[j];
						}
					
						return new string (r);
					}	
				}
			}

			char [] rn = new char[num + 1];
			for(int i = 0; i < num; i++) {
				rn [i] = (char)buf [i];
			}
			rn [num] = '\0';

			return new string (rn);
		}
	
		private float[] readFloats(BinaryReader br, int num)
		{
			float[] tmp = new float[num];
			for (int i = 0; i < num; i++) {
				tmp [i] = br.ReadSingle ();
			}

			return tmp;
		}
	}
}

