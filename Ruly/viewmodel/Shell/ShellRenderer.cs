using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Android.Views;

using Android.Widget;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

using Ruly.model;

namespace Ruly.viewmodel
{
	public class ShellRenderer: Java.Lang.Object, GLSurfaceView.IRenderer
	{
		private string TAG = "Ruly.ShellRenderer";

		// GPU configuration
		private bool	mNpot;

		// shader
		private Dictionary<String, RenderTarget> mRT;
		private Dictionary<String, GLSL>	mGLSL;
		
		// tmp buffers
		public float[]  mBoneMatrix = new float[16 * 256];	// ad-hock number: will be fixed to mMaxBone
		private float[] mLightDir = new float[3];
		private float[] mDifAmb = new float[4];
		private int[]	mTexSize = new int[1];

		public void OnDrawFrame (IGL10 gl)
		{
			mLightDir[0] = -0.5f; mLightDir[1] = -1.0f; mLightDir[2] = -0.5f;	// in left-handed region
			Vector.normalize(mLightDir);

			mRT["screen"].switchTargetFrameBuffer();
			GLES20.GlClear(GL10.GlColorBufferBit | GL10.GlDepthBufferBit);

			////////////////////////////////////////////////////////////////////
			//// draw models
			foreach (var shell in ShellViewModel.Shells) {
				if(shell.Surface.Loaded) {
					foreach(var rs in shell.RenderSets) {
						mRT[rs.target].switchTargetFrameBuffer();
						GLSL glsl = mGLSL[rs.shader];

						GLES20.GlUseProgram(glsl.mProgram);

						// Projection Matrix
						GLES20.GlUniformMatrix4fv(glsl.muPMatrix, 1, false, ShellViewModel.ProjectionMatrix, 0);

						// LightPosition
						GLES20.GlUniform3fv(glsl.muLightDir, 1, mLightDir, 0);

						bindBuffer(shell.Surface, glsl);
						if(!rs.shader.EndsWith("alpha")) {
							drawNonAlpha(shell.Surface, glsl);							
							drawAlpha(shell.Surface, glsl, false);						
						} else {
							drawAlpha(shell.Surface, glsl, true);							
						}
					}
				}
			}

			GLES20.GlFlush();
			checkGlError(TAG);
//			mCoreLogic.onDraw(pos);
		}

		public void OnSurfaceChanged (IGL10 gl, int width, int height)
		{
			ShellViewModel.Width = width;
			ShellViewModel.Height = height;
			ShellViewModel.setDefaultCamera ();
			GLES20.GlViewport(0, 0, width, height);
			Log.Debug ("Ruly", "OnSurfaceChanged " + width.ToString () + " X " + height.ToString ());
		}

		public void OnSurfaceCreated (IGL10 gl, EGLConfig config)
		{
			// GL configurations
			int bonenum = 48;
			GLES20.GlGetIntegerv(GLES20.GlMaxTextureSize, mTexSize, 0);
			ShellViewModel.GLConfig = bonenum;
			mNpot = hasExt("GL_OES_texture_npot");

			// initialize
			GLES20.GlClearColor(1, 1, 1, 1);
			GLES20.GlEnable(GLES20.GlDepthTest);
			GLES20.GlEnable(GLES20.GlBlend);
//			GLES20.GlEnable (GLES20.GlCullFaceMode);
			checkGlError ("GlEnable");

			// GLUtils.texImage2D generates premultiplied-alpha texture. so we use GL_ONE instead of GL_ALPHA
			GLES20.GlBlendFunc(GL10.GlOne, GL10.GlOneMinusSrcAlpha);
			GLES20.GlDepthFunc(GLES20.GlLequal);
			GLES20.GlFrontFace(GLES20.GlCw);
			checkGlError ("GlMiscSettings");

			// shader programs
			mGLSL = new Dictionary<String, GLSL>();

			mGLSL.Add("builtin:nomotion",
				new GLSL(Util.ReadAssetString("shader/vs_notex.vsh"), Util.ReadAssetString("shader/fs_notex.fsh")));

			mGLSL.Add("builtin:nomotion_alpha",
				new GLSL(Util.ReadAssetString("shader/vs_notex.vsh"), Util.ReadAssetString("shader/fs_notex_alpha.fsh")));

			// render targets
			mRT = new Dictionary<String, RenderTarget>();
			mRT.Add("screen", new RenderTarget());

			GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, 0);

			// Load ends
			ShellViewModel.Shells [0].Surface.Loaded = true;	// ad-hock
		}

		private bool hasExt(String extension) {
			String extensions = " " + GLES20.GlGetString(GLES20.GlExtensions) + " ";
			return extensions.IndexOf(" " + extension + " ") >= 0;
		}

		private void checkGlError(String op) {
			int error;
			while ((error = GLES20.GlGetError()) != GLES20.GlNoError) {
				Log.Error(TAG, op + ": glError " + error + " " + GLU.GluErrorString(error));
			}
		}

		private void drawNonAlpha(ShellSurface surface, GLSL glsl) {
			var mats = surface.material;

			int max = mats.Count;

			// draw non-alpha material
//			GLES20.GlEnable(GLES20.GlCullFace);
			GLES20.GlEnable (2884);
			GLES20.GlCullFace (GLES20.GlBack);
			GLES20.GlDisable(GLES20.GlBlend);
			for (int r = 0; r < max; r++) {
				Material mat = mats[r];
				TexInfo tb = null;
				if (mat.texture != null) {
					tb = TextureFile.FetchTexInfo(mat.texture);
				}
				if(mat.diffuse_color[3] >= 1.0 && (tb == null || !tb.has_alpha)) {
					drawOneMaterial(glsl, surface, mat);
				}
			}
		}

		private void drawAlpha(ShellSurface surface, GLSL glsl, bool alpha_test) {
			int max = surface.material.Count ();

			// draw alpha material
			GLES20.GlEnable(GLES20.GlBlend);
			for (int r = 0; r < max; r++) {
				Material mat = surface.material[r];
				TexInfo tb = null;
				if (mat.texture != null) {
					tb = TextureFile.FetchTexInfo(mat.texture);
				}
				if(alpha_test) {
					if(tb != null && tb.needs_alpha_test) {
						if(mat.diffuse_color[3] < 1.0) {
//							GLES20.GlDisable(GLES20.GL_CULL_FACE);
							GLES20.GlDisable (2884);
						} else {
//							GLES20.GlEnable(GLES20.GL_CULL_FACE);
							GLES20.GlEnable (2884);
						}
						drawOneMaterial(glsl, surface, mat);					
					}
				} else {
					if(tb != null) { // has texture
						if(!tb.needs_alpha_test) {
							if(tb.has_alpha) {
								if(mat.diffuse_color[3] < 1.0) {
//									GLES20.GlDisable(GLES20.GL_CULL_FACE);
									GLES20.GlDisable (2884);
								} else {
//									GLES20.GlEnable(GLES20.GL_CULL_FACE);						
									GLES20.GlEnable (2884);
								}
								drawOneMaterial(glsl, surface, mat);						
							} else if(mat.diffuse_color[3] < 1.0) {
//								GLES20.GlDisable(GLES20.GL_CULL_FACE);
								GLES20.GlDisable (2884);
								drawOneMaterial(glsl, surface, mat);
							}
						}
					} else {
						if(mat.diffuse_color[3] < 1.0) {
//							GLES20.GlDisable(GLES20.GL_CULL_FACE);
							GLES20.GlDisable (2884);
							drawOneMaterial(glsl, surface, mat);
						}
					}
				}
			}
		}

		private void drawOneMaterial(GLSL glsl, ShellSurface surface, Material mat) {
			// initialize color
			for(int i = 0; i < mDifAmb.Count(); i++) {
				mDifAmb[i] = 1.0f;
			}

			// diffusion and ambient
			float wi = 0.6f;	// light color = (0.6, 0.6, 0.6)
			for(int i = 0; i < 3; i++) {
				mDifAmb[i] *= mat.diffuse_color[i] * wi + mat.emmisive_color[i];
			}
			mDifAmb[3] *= mat.diffuse_color[3];
			Vector.min(mDifAmb, 1.0f);
			GLES20.GlUniform4fv(glsl.muDif, 1, mDifAmb, 0);

			// toon
			GLES20.GlUniform1i(glsl.msToonSampler, 0);
			GLES20.GlActiveTexture(GLES20.GlTexture0);
			GLES20.GlBindTexture(GLES20.GlTexture2d, TextureFile.FetchTexInfo(surface.toon_name[mat.toon_index]).tex);

			// texture
			GLES20.GlUniform1i(glsl.msTextureSampler, 1);
			GLES20.GlActiveTexture(GLES20.GlTexture1);
			if (mat.texture != null) {
				TexInfo tb = TextureFile.FetchTexInfo(mat.texture);
				if(tb != null) {
					GLES20.GlBindTexture(GLES20.GlTexture2d, tb.tex);
				} else {	// avoid crash
					GLES20.GlBindTexture(GLES20.GlTexture2d, TextureFile.FetchTexInfo(surface.toon_name[0]).tex);	// white texture using toon0.bmp
					for(int i = 0; i < 3; i++) {	// for emulate premultiplied alpha
						mDifAmb[i] *= mat.diffuse_color[3];
					}
				}
			} else {
				GLES20.GlBindTexture(GLES20.GlTexture2d, TextureFile.FetchTexInfo(surface.toon_name[0]).tex);	// white texture using toon0.bmp
				for(int i = 0; i < 3; i++) {	// for emulate premultiplied alpha
					mDifAmb[i] *= mat.diffuse_color[3];
				}
			}

			// draw
			surface.IndexBuffer.Position (mat.face_vert_offset);
			GLES20.GlDrawElements(GLES20.GlTriangles, mat.face_vert_count, GLES20.GlUnsignedShort, surface.IndexBuffer);
			checkGlError("glDrawElements");
		}

		private void bindBuffer(ShellSurface surface, GLSL glsl) {
			GLES20.GlEnableVertexAttribArray(glsl.maPositionHandle);
			GLES20.GlVertexAttribPointer (glsl.maPositionHandle, 3, GLES20.GlFloat, false, 0, surface.VertexBuffer);

			GLES20.GlEnableVertexAttribArray(glsl.maUvHandle);
			GLES20.GlVertexAttribPointer (glsl.maUvHandle, 2, GLES20.GlFloat, false, 0, surface.UvBuffer);
			checkGlError("drawGLES20 VertexAttribPointer vertex");
		}
	}
	
}
