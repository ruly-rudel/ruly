using System.Collections.Generic;
//using Sce.PlayStation.Core;
//using Sce.PlayStation.Core.Graphics;
using System.Linq;
using Android.Util;
using Java.Nio;

using Ruly.viewmodel;

namespace Ruly.model
{
	public class Vector2 {
		public float x;
		public float y;

		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public class Vector3 {
		public float x;
		public float y;
		public float z;

		public Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	public class Vector4 {
		public float x;
		public float y;
		public float z;
		public float w;

		public Vector4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
	}

	public class Material {
		public float[]		diffuse_color;
		public float 		power;
		public float[]		specular_color;
		public float[]		emmisive_color;
		public byte			toon_index;
		public byte			edge_flag;
		public int			face_vert_count;
		public string 		texture;
		
//		public float[]		vertex;
//		public TexInfo		texture_info;

		public int 			face_vert_offset;
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
		public Vector3[]	face_vert_offset;
	}

	public class TexInfo {
		public int		tex;
		public bool		has_alpha;
		public bool		needs_alpha_test;
	}
	
	public class ShellSurface
	{
		public bool TextureLoaded {
			get;
			set;
		}


		public List<Material>				material	{ set; get; }
		public string[]						toon_name;
//		public TexInfo[]					toon		{ get; set; }
//		public Dictionary<string, TexInfo>	texture		{ get; set; }

		public Java.Nio.FloatBuffer VertexBuffer;

		public Java.Nio.FloatBuffer NormalBuffer;

		public Java.Nio.FloatBuffer UvBuffer;

		public Java.Nio.ShortBuffer IndexBuffer;
				
		public ShellSurface ()
		{
			material = new List<Material>();
//			toon = new TexInfo[11];
//			texture = new Dictionary<string, TexInfo> ();
			TextureLoaded = false;
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

		public List<RenderSet> RenderSets {
			get;
			private set;
		}

		public Shell()
		{
			Surface = null;
			RenderSets = new List<RenderSet> ();
		}

		public void LoadPMD(string root, string dir, string name)
		{
			string path = root + dir + name;
			var surface = new PMD (path);
			RenderSets.Add(new RenderSet("builtin:nomotion", "screen"));

			// create buffers for render
			ByteBuffer buf = ByteBuffer.AllocateDirect (surface.Vertex.Length * sizeof(float));
			buf.Order (ByteOrder.NativeOrder ());
			surface.VertexBuffer = buf.AsFloatBuffer() as FloatBuffer;
			surface.VertexBuffer.Put (surface.Vertex);
			surface.VertexBuffer.Position (0);

			buf = ByteBuffer.AllocateDirect (surface.Uv.Length * sizeof(float));
			buf.Order (ByteOrder.NativeOrder ());
			surface.UvBuffer = buf.AsFloatBuffer() as Java.Nio.FloatBuffer;
			surface.UvBuffer.Put (surface.Uv);
			surface.UvBuffer.Position (0);

			buf = ByteBuffer.AllocateDirect (surface.Index.Length * sizeof(short));
			buf.Order (ByteOrder.NativeOrder ());
			surface.IndexBuffer = buf.AsShortBuffer() as Java.Nio.ShortBuffer;
			surface.IndexBuffer.Put (surface.Index);
			surface.IndexBuffer.Position (0);

			// create texture entry for future texture read
			foreach (var m in surface.material) {
				if (m.texture != null) {
					m.texture = TextureFile.SearchTextureFilePath (root, dir, m.texture);
					if (m.texture != null) {
						TextureFile.AddTexture (m.texture);
					}
				}
			}
			for (int i = 0; i < surface.toon_name.Length; i++) {
				surface.toon_name [i] = TextureFile.SearchTextureFilePath (root, dir, surface.toon_name [i]);
				if (surface.toon_name [i] != null) {
					TextureFile.AddTexture (surface.toon_name [i]);
				}
			}

			Surface = surface;
			Log.Debug ("Shell", "PMD load ends.");

			//			RenderSets.Add(new RenderSet("builtin:nomotion_alpha", "screen"));
		}
	}
}

