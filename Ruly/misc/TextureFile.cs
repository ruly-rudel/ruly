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

namespace Ruly
{
	public class TextureFile
	{
		public static Dictionary<string, Texture2D> m_texture = new Dictionary<string, Texture2D>();

		public static Texture2D load (string basedir, string src)
		{
			return null;
		}

		public static Texture2D load (string basedir, byte[] src)
//		public static Texture2D load (string basedir, string src)
		{
//			CP932 cp = new CP932();
//
//			String s = cp.GetString(src);
//			String[] sp = s.Split("*".ToCharArray());
//			string path = Path.Combine(basedir, sp[0]);
//			
//			if(!File.Exists(path)) {
//				path = Path.Combine("/Application/toon/", sp[0]);
//				if(!File.Exists(path)) {
//					Log.d ("TextureFile", "cannot find texture file " + Path.Combine(basedir, sp[0]) + " or " + path);
//					return null;
//				}
//			}
//			
//			Log.d("TextureFile", "Loading " + path);
//			try {
//				return m_texture[path];
//			} catch(Exception e) {
//				string ext = Path.GetExtension(path);
//				Texture2D ret;
//				if(ext == ".tga") {
//					ret = texture2DTGA(path, 1);
//				} else {
//					ret = new Texture2D(path, false);
//				}
//				m_texture[path] = ret;
//				return ret;
//			}		

			return null;
		}
//		
//		
//		private static Texture2D texture2DTGA(string path, int scale) {
//			FileStream fs = File.OpenRead (path);
//			BinaryReader br = new BinaryReader(fs);
//
//			// read header
//			br.ReadBytes (2);
//			int type = br.ReadByte();
//			if (type != 2 && type != 10) {
//				Log.d("TextureFile", "Unsupported TGA TYPE: " + type.ToString());
//				Log.d("TextureFile", "fail to create png from tga: " + path);
//				
//				br.Close();
//				fs.Close();
//				return null;
//			}
//			fs.Seek(12, SeekOrigin.Begin);
//			short w = br.ReadInt16();
//			short h = br.ReadInt16();
//			byte depth = br.ReadByte();
//			byte mode = br.ReadByte();
//			
//			Texture2D tx = new Texture2D(w, h, false, PixelFormat.Rgba);
//			
//			// create bitmap
//			if (depth == 24 || depth == 32) {
//				byte[] buf = loadTGA(br, w, h, depth, type, mode);
//								
//				tx.SetPixels(0, buf);
//			} else {
//				Log.d("TextureFile", "Unsupported TGA depth:" + depth.ToString());
//				Log.d("TextureFile", "fail to create png from tga: " + path);					
//			}
//				
//			br.Close ();
//			fs.Close ();
//			
//			return tx;
//		}
//
//		private static byte[] loadTGA(BinaryReader br, short w, short h, byte depth, int type, byte mode)
//		{
//			if(type == 2) {
//				return loadNonCompressedTGA(br, w, h, depth, mode);
//			} else {
//				return loadRLETGA(br, w, h, depth, mode);
//			}
//		}
//		
//		private static byte[] loadNonCompressedTGA(BinaryReader br, short w, short h, byte depth, byte mode)
//		{
//			byte[] buf = new byte[w * h * 4];
//			byte[] color = new byte[4];
//			for (int i = 0; i < h; i++) {
//				for (int j = 0; j < w; j++) {
//					getPixel(br, color, depth);					
//					setPixel(buf, w, h, j, i, mode, color, 1);
//				}
//			}
//			
//			return buf;
//		}
//		
//		private static byte[] loadRLETGA(BinaryReader br, short w, short h, byte depth, byte mode) {
//			int pos = 0;
//			byte[] buf = new byte[w * h * 4];
//			byte[] color = new byte[4];
//			while (pos < w * h) {
//				byte header = br.ReadByte();
//				if (header < 128) { // normal pixel
//					for (int i = 0; i < header + 1; i++) {
//						getPixel(br, color, depth);
//						setPixel(buf, pos++, w, h, mode, color, 1);
//					}
//				} else { // RLE pixel
//					header -= 127;
//					getPixel(br, color, depth);
//										
//					for (int i = 0; i < header; i++) {
//						setPixel(buf, pos++, w, h, mode, color, 1);
//					}
//				}
//			}
//			
//			return buf;
//		}
//		
//		private static byte[] getPixel(BinaryReader br, byte[] buf, byte depth)
//		{
//			buf[2] = br.ReadByte();
//			buf[1] = br.ReadByte();
//			buf[0] = br.ReadByte();
//			buf[3] = 255;
//			if(depth == 32) {
//				buf[3] = br.ReadByte();
//			}
//			
//			return buf;
//		}
//		
//		private static void setPixel(byte[] buf, int pos, short w, short h, int mode, byte[] color, int scale) {
//			int x, y;
//			x = pos % (w * scale);
//			y = pos / (w * scale);
//			setPixel(buf, w, h, x, y, mode, color, scale);
//		}
//
//		private static void setPixel(byte[] buf, short w, short h, int x, int y, int mode, byte[] color, int scale)
//		{
//			if ((mode & 0x10) != 0) {
//				x = (w * scale) - x - 1;
//			}
//			if ((mode & 0x20) == 0) {
//				y = (h * scale) - y - 1;
//			}
//	
//			if ((x % scale) == 0 && (y % scale) == 0) {
//				buf[y * w * 4 + x * 4 + 0] = color[0];
//				buf[y * w * 4 + x * 4 + 1] = color[1];
//				buf[y * w * 4 + x * 4 + 2] = color[2];
//				buf[y * w * 4 + x * 4 + 3] = color[3];	
//			}			
//		}
	}
}

