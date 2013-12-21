using System;
using System.IO;
using System.Collections.Generic;
//using Sce.PlayStation.Core.Graphics;
//using Sce.PlayStation.Core.Imaging;
//using I18N.CJK;
using Android.Opengl;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

using Ruly.model;
using Android.Util;
using Android.Graphics;
using Android.App;

namespace Ruly
{
	public class TextureFile
	{
		public static Dictionary<string, TexInfo> m_texture = new Dictionary<string, TexInfo>();

//		public static Texture2D load (string basedir, byte[] src)
		public static TexInfo load (string basedir, string src)
		{
			var asset = Application.Context.ApplicationContext.Assets;
			String s = src;
			String[] sp = s.Split("*".ToCharArray());
			string path = System.IO.Path.Combine(basedir, sp[0]);
			
			if(!File.Exists(path)) {
				path = System.IO.Path.Combine("toon/", sp[0]);
				if(!File.Exists(path)) {
					Log.Debug("TextureFile", "cannot find texture file " + System.IO.Path.Combine(basedir, sp[0]) + " or " + path);
					return null;
				}
			}
			
			Log.Debug("TextureFile", "Loading " + path);
			try {
				return m_texture[path];
			} catch(Exception e) {
				string ext = System.IO.Path.GetExtension(path);
				TexInfo ret;
				if(ext == ".tga") {
					ret = texture2DTGA(path, 1);
				} else {
					using (var bs = Application.Context.ApplicationContext.Assets.Open (path)) {
						ret = CreateTexInfoFromBitmap (BitmapFactory.DecodeStream (bs));
					}
				}
				m_texture[path] = ret;
				return ret;
			}		

			return null;
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
			
			return (int)((buf [3] << 24) | (buf [2] << 16) | (buf [1] << 8) | buf [0]);
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
//				buf[y * w * 4 + x * 4 + 0] = color[0];
//				buf[y * w * 4 + x * 4 + 1] = color[1];
//				buf[y * w * 4 + x * 4 + 2] = color[2];
//				buf[y * w * 4 + x * 4 + 3] = color[3];	
			}			
		}
	}
}

