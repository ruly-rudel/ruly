using System;
using System.IO;
using System.Collections.Generic;
using Android.Opengl;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

using Ruly.model;
using Android.Util;
using Android.Graphics;
using Android.App;

using System.Drawing;

namespace Ruly.viewmodel
{
	public class TexInfo {
		public int		tex;
		public bool		has_alpha;
		public bool		needs_alpha_test;
	}

	public class TextureFile
	{
		public static Dictionary<string, TexInfo> m_texture = new Dictionary<string, TexInfo>();

		public static string SearchTextureFilePath(string root, string dir, string src)
		{
			String[] sp = src.Split("*".ToCharArray());
			string path = System.IO.Path.Combine(root + dir, sp[0]);


			if (!File.Exists (path) && !File.Exists(System.IO.Path.ChangeExtension(path, "png"))) {
				path = System.IO.Path.Combine (root + "/toon/", sp [0]);
				if (!File.Exists (path)) {
					Log.Debug ("TextureFile", "cannot find texture file " + System.IO.Path.Combine (dir, sp [0]) + " or " + path);
					return null;
				}
			} else {
				// ad-hock: pre-generated png file
				if(File.Exists(System.IO.Path.ChangeExtension(path, "png"))) {
					path = System.IO.Path.ChangeExtension(path, "png");
				}
			}

			if (path == null)
				Log.Debug ("Ruly.TextureFile", "texture path is null");
			else
				Log.Debug ("Ruly.TextureFile", "texture " + src + " at " + path);

			return path;
		}

		public static void AddTexture(string path)
		{
			m_texture [path] = null;
		}
		
		public static TexInfo FetchTexInfo (string path)
		{
			if (m_texture [path] == null) {
				Log.Debug("TextureFile", "Loading " + path);
				string ext = System.IO.Path.GetExtension(path);
				TexInfo ret;
				if(ext == ".tga") {
					ret = texture2DTGA(path, 1);
				} else {
					using (var bs = File.OpenRead (path))
					using (var bbs = new BufferedStream(bs)) {
						ret = CreateTexInfoFromBitmap (BitmapFactory.DecodeStream (bbs));
					}
				}
				m_texture[path] = ret;
			}

			return m_texture [path];
		}
		
		
		private static TexInfo texture2DTGA(string path, int scale) {
			FileStream fs = File.OpenRead (path);
			BinaryReader br = new BinaryReader(fs);

			// read header
			br.ReadBytes (2);
			int type = br.ReadByte();
			if (type != 2 && type != 10) {
				Log.Debug("TextureFile", "Unsupported TGA TYPE: " + type.ToString());
				Log.Debug("TextureFile", "fail to create png from tga: " + path);
				
				br.Close();
				fs.Close();
				return null;
			}
			fs.Seek(12, SeekOrigin.Begin);
			short w = br.ReadInt16();
			short h = br.ReadInt16();
			byte depth = br.ReadByte();
			byte mode = br.ReadByte();
			
			TexInfo tx = null;

			// create bitmap
			if (depth == 24 || depth == 32) {
				int[] buf = loadTGA(br, w, h, depth, type, mode);
								
				Bitmap bitmap = Bitmap.CreateBitmap (buf, w, h, Bitmap.Config.Argb8888);
				tx = CreateTexInfoFromBitmap (bitmap);
			} else {
				Log.Debug("TextureFile", "Unsupported TGA depth:" + depth.ToString());
				Log.Debug("TextureFile", "fail to create png from tga: " + path);					
			}
				
			br.Close ();
			fs.Close ();
			
			return tx;
		}

		private static TexInfo CreateTexInfoFromBitmap(Bitmap bitmap)
		{
			TexInfo ret = null;
			if (bitmap != null) {
				ret = new TexInfo ();

				ret.has_alpha = bitmap.HasAlpha;
				ret.needs_alpha_test = false;	// ad-hock

				GLES20.GlPixelStorei(GLES20.GlUnpackAlignment, 1);
				int[] tex = new int[1];
				GLES20.GlGenTextures(1, tex, 0);
				ret.tex = tex [0];

				GLES20.GlBindTexture(GLES20.GlTexture2d, ret.tex);
				GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlClampToEdge);
				GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlClampToEdge);
				GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);
				GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlLinear);

				GLUtils.TexImage2D(GLES20.GlTexture2d, 0, bitmap, 0);
				bitmap.Recycle ();
			}

			return ret;
		}

		private static int[] loadTGA(BinaryReader br, short w, short h, byte depth, int type, byte mode)
		{
			if(type == 2) {
				return loadNonCompressedTGA(br, w, h, depth, mode);
			} else {
				return loadRLETGA(br, w, h, depth, mode);
			}
		}
		
		private static int[] loadNonCompressedTGA(BinaryReader br, short w, short h, byte depth, byte mode)
		{
			int[] buf = new int[w * h];
			byte[] color = new byte[4];
			for (int i = 0; i < h; i++) {
				for (int j = 0; j < w; j++) {
					int c = getPixel(br, color, depth);
					setPixel(buf, w, h, j, i, mode, c, 1);
				}
			}
			
			return buf;
		}

		private static int[] loadRLETGA(BinaryReader br, short w, short h, byte depth, byte mode) {
			int pos = 0;
			int[] buf = new int[w * h];
			byte[] color = new byte[4];
			while (pos < w * h) {
				byte header = br.ReadByte();
				if (header < 128) { // normal pixel
					for (int i = 0; i < header + 1; i++) {
						int c = getPixel(br, color, depth);
						setPixel(buf, pos++, w, h, mode, c, 1);
					}
				} else { // RLE pixel
					header -= 127;
					int c = getPixel(br, color, depth);
										
					for (int i = 0; i < header; i++) {
						setPixel(buf, pos++, w, h, mode, c, 1);
					}
				}
			}
			
			return buf;
		}
		
		private static int getPixel(BinaryReader br, byte[] buf, byte depth)
		{
			buf[2] = br.ReadByte();
			buf[1] = br.ReadByte();
			buf[0] = br.ReadByte();
			buf[3] = 255;
			if(depth == 32) {
				buf[3] = br.ReadByte();
			}
			
			return (int)((buf [3] << 24) | (buf [0] << 16) | (buf [1] << 8) | buf [2]);
		}
		
		private static void setPixel(int[] buf, int pos, short w, short h, int mode, int color, int scale) {
			int x, y;
			x = pos % (w * scale);
			y = pos / (w * scale);
			setPixel(buf, w, h, x, y, mode, color, scale);
		}

		private static void setPixel(int[] buf, short w, short h, int x, int y, int mode, int color, int scale)
		{
			if ((mode & 0x10) != 0) {
				x = (w * scale) - x - 1;
			}
			if ((mode & 0x20) == 0) {
				y = (h * scale) - y - 1;
			}
	
			if ((x % scale) == 0 && (y % scale) == 0) {
				buf [y * w + x] = color;
			}			
		}
	}
}

