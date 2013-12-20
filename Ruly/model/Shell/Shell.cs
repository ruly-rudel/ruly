using System.Collections.Generic;
//using Sce.PlayStation.Core;
//using Sce.PlayStation.Core.Graphics;
using System.Linq;
using Android.Util;

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

	public class Texture2D {
		public int id {
			get;
			set;
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
		
		public float[]		vertex;
		public Texture2D 	texture2d;

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

		public float[]		Vertex;
		public float[]		Normal;
		public float[]		Uv;
		public ushort[]	Index;
		public List<Material>				material	{ set; get; }
		public Texture2D[]					toon		{ get; set; }
		public Dictionary<string, TexInfo>	texture		{ get; set; }

		public Java.Nio.FloatBuffer VertexBuffer;

		public Java.Nio.FloatBuffer NormalBuffer;

		public Java.Nio.ShortBuffer IndexBuffer;
				
		public ShellSurface ()
		{
			material = new List<Material>();
			toon = new Texture2D[11];
			texture = new Dictionary<string, TexInfo> ();
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

		public void LoadPMD(string path)
		{
			Surface = new PMD (path);
			RenderSets.Add(new RenderSet("builtin:nomotion", "screen"));

			var buf = Java.Nio.ByteBuffer.AllocateDirect (Surface.Vertex.Count () * 4);
			Surface.VertexBuffer = buf.AsFloatBuffer() as Java.Nio.FloatBuffer;
			foreach (var i in Surface.Vertex) {
				Surface.VertexBuffer.Put (i);
			}
			Surface.VertexBuffer.Position (0);

			buf = Java.Nio.ByteBuffer.AllocateDirect (Surface.Index.Count () * 2);
			Surface.IndexBuffer = buf.AsShortBuffer() as Java.Nio.ShortBuffer;
			foreach (var i in Surface.Index) {
				Surface.IndexBuffer.Put ((short)i);
			}
			Surface.IndexBuffer.Position (0);

			Log.Debug ("Shell", "PMD load ends.");

			//			RenderSets.Add(new RenderSet("builtin:nomotion_alpha", "screen"));
		}
	}
}

