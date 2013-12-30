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

	public abstract class ShellMotion
	{
		protected Dictionary<string, List<BoneMotion>> Bone {
			get;
			set;
		}

		protected Dictionary<string, List<Morphing>> Morph {
			get;
			set;
		}

		public int max_frame {
			get;
			protected set;
		}

		public abstract BoneMotion BoneMotionAt (RenderBone b, float time);
	}
	
}
