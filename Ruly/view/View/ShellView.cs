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

using Ruly.viewmodel;
using Ruly.model;

namespace Ruly.view
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
//			Vector.normalize(mLightDir);

			mRT["screen"].switchTargetFrameBuffer();
			GLES20.GlClear(GL10.GlColorBufferBit | GL10.GlDepthBufferBit);
//			initializeAllTexture(false);
//			initializeAllSkinningTexture();

//			int pos = mCoreLogic.applyCurrentMotion();

			////////////////////////////////////////////////////////////////////
			//// draw models
			if (ShellViewModel.Shells != null) {
				foreach (var shell in ShellViewModel.Shells) {
					if(shell.Surface.TextureLoaded) {
						foreach(var rs in shell.RenderSets) {
							mRT[rs.target].switchTargetFrameBuffer();
							GLSL glsl = mGLSL[rs.shader];

							GLES20.GlUseProgram(glsl.mProgram);

							// Projection Matrix
							GLES20.GlUniformMatrix4fv(glsl.muPMatrix, 1, false, ShellViewModel.ProjectionMatrix, 0);

							// LightPosition
//							GLES20.GlUniform3fv(glsl.muLightDir, 1, mLightDir, 0);
//
//							GLES20.GlUniform1i(glsl.msToonSampler, 0);
//							GLES20.GlUniform1i(glsl.msTextureSampler, 1);
//							GLES20.GlUniform1i(glsl.msSphereSampler, 2);

							bindBuffer(shell.Surface, glsl);
							drawAlpha(shell.Surface, glsl, true);							
//							if(!rs.shader.EndsWith("alpha")) {
//								drawNonAlpha(shell.Surface, glsl);							
//								drawAlpha(shell.Surface, glsl, false);						
//							} else {
//								drawAlpha(shell.Surface, glsl, true);							
//							}
						}
					}
				}
			}

//			////////////////////////////////////////////////////////////////////
//			//// draw BG
//			String bg = mCoreLogic.getBG();
//			if(bg != null) {
//				GLSL glsl = mGLSL.get("builtin:bg");
//				GLES20.glUseProgram(glsl.mProgram);
//				GLES20.glUniform1i(glsl.msTextureSampler, 1);
//
//				bindBgBuffer(glsl);
//				drawBg(bg);
//			}

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

//			mGLSL.Add("builtin:default", 
//				new GLSL(String.Format(Util.ReadAssetString("shader/vs.vsh"), bonenum), Util.ReadAssetString("shader/fs.fsh")));
//			mGLSL.Add("builtin:default_alpha", 
//				new GLSL(String.Format(mCoreLogic.getRawResourceString(R.raw.vs), bonenum), mCoreLogic.getRawResourceString(R.raw.fs_alpha)));
//			mGLSL.Add("builtin:default_shadow", 
//				new GLSL(String.Format(mCoreLogic.getRawResourceString(R.raw.vs_shadow), bonenum), mCoreLogic.getRawResourceString(R.raw.fs_shadow)));
			mGLSL.Add("builtin:nomotion",
				new GLSL(Util.ReadAssetString("shader/vs_notex.vsh"), Util.ReadAssetString("shader/fs_notex.fsh")));
//			mGLSL.Add("builtin:nomotion_alpha",
//				new GLSL(Util.ReadAssetString("shader/vs_notex.vsh"), Util.ReadAssetString("shader/fs_notex.fsh")));
//			mGLSL.Add("builtin:bg",
//				new GLSL(Util.ReadAssetString("shader/vs_bg.vsh"), Util.ReadAssetString("shader/fs_bg.fsh")));
//			mGLSL.Add("builtin:post_diffusion", 
//				new GLSL(mCoreLogic.getRawResourceString(R.raw.vs_bg), mCoreLogic.getRawResourceString(R.raw.fs_post_diffusion)));

			// render targets
			mRT = new Dictionary<String, RenderTarget>();
			mRT.Add("screen", new RenderTarget());

			GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, 0);

			// bind textures
//			initializeAllTexture(true);
//			initializeAllSkinningTexture();
			ShellViewModel.Shells [0].Surface.TextureLoaded = true;	// ad-hock
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
//			ArrayList<Material> rendar = surface.mAnimation ? surface.mRendarList : surface.material;
			var mats = surface.material;

			int max = mats.Count;

			// draw non-alpha material
//			GLES20.GlEnable(GLES20.GlCullFace);
			GLES20.GlDisable(GLES20.GlBlend);
			for (int r = 0; r < max; r++) {
				Material mat = mats[r];
//				TexInfo tb = null;
//				if (mat.texture != null) {
//					tb = surface.texture[mat.texture];
//				}
//				if(mat.diffuse_color[3] >= 1.0 && (tb == null || !tb.has_alpha)) {
//					drawOneMaterial(glsl, surface, mat);
//				}
				drawOneMaterial(glsl, surface, mat);
			}
		}

		private void drawAlpha(ShellSurface miku, GLSL glsl, bool alpha_test) {
			drawNonAlpha (miku, glsl);
		}
//		private void drawAlpha(MikuModel miku, GLSL glsl, boolean alpha_test) {
//			ArrayList<Material> rendar = miku.mAnimation ? miku.mRendarList : miku.mMaterial;
//
//			int max = rendar.size();
//
//			// draw alpha material
//			GLES20.glEnable(GLES20.GL_BLEND);
//			for (int r = 0; r < max; r++) {
//				Material mat = rendar.get(r);
//				TexInfo tb = null;
//				if (mat.texture != null) {
//					tb = miku.mTexture.get(mat.texture);
//				}
//				if(alpha_test) {
//					if(tb != null && tb.needs_alpha_test) {
//						if(mat.diffuse_color[3] < 1.0) {
//							GLES20.glDisable(GLES20.GL_CULL_FACE);
//						} else {
//							GLES20.glEnable(GLES20.GL_CULL_FACE);						
//						}
//						drawOneMaterial(glsl, miku, mat);					
//					}
//				} else {
//					if(tb != null) { // has texture
//						if(!tb.needs_alpha_test) {
//							if(tb.has_alpha) {
//								if(mat.diffuse_color[3] < 1.0) {
//									GLES20.glDisable(GLES20.GL_CULL_FACE);
//								} else {
//									GLES20.glEnable(GLES20.GL_CULL_FACE);						
//								}
//								drawOneMaterial(glsl, miku, mat);						
//							} else if(mat.diffuse_color[3] < 1.0) {
//								GLES20.glDisable(GLES20.GL_CULL_FACE);
//								drawOneMaterial(glsl, miku, mat);
//							}
//						}
//					} else {
//						if(mat.diffuse_color[3] < 1.0) {
//							GLES20.glDisable(GLES20.GL_CULL_FACE);
//							drawOneMaterial(glsl, miku, mat);
//						}
//					}
//				}
//			}
//		}

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
//			Vector.min(mDifAmb, 1.0f);
			GLES20.GlUniform4fv(glsl.muDif, 1, mDifAmb, 0);

//			// speculation
//			if (glsl.muPow >= 0) {
//				GLES20.GlUniform4f(glsl.muSpec, mat.specular_color[0], mat.specular_color[1], mat.specular_color[2], 0);
//				GLES20.GlUniform1f(glsl.muPow, mat.power);
//			}

			// draw
//			surface.Index.position(mat.face_vert_offset);
			surface.IndexBuffer.Position (mat.face_vert_offset);
			GLES20.GlDrawElements(GLES20.GlTriangles, mat.face_vert_count, GLES20.GlUnsignedShort, surface.IndexBuffer);
			checkGlError("glDrawElements");
		}


		private void bindBuffer(ShellSurface surface, GLSL glsl) {
			GLES20.GlEnableVertexAttribArray(glsl.maPositionHandle);

			GLES20.GlVertexAttribPointer (glsl.maPositionHandle, 3, GLES20.GlFloat, false, 0, surface.VertexBuffer);
			checkGlError("drawGLES20 VertexAttribPointer vertex");

//			GLES20.GlEnableVertexAttribArray(glsl.maNormalHandle);
//			surface.NormalBuffer = Java.Nio.FloatBuffer.FromArray (surface.Normal) as Java.Nio.FloatBuffer;
//			GLES20.GlVertexAttribPointer (glsl.maNormalHandle, 3, GLES20.GlFloat, false, 0, surface.NormalBuffer);
//			checkGlError("drawGLES20 VertexAttribPointer normal");


		}


	}

	public class ShellView : GLSurfaceView
	{
		ShellRenderer shellRenderer;

		public ShellView (Context context) :
		base (context)
		{
			Initialize ();
		}

		public ShellView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}

		void Initialize ()
		{
//			ShellViewModel.Width = Width;
//			ShellViewModel.Height = Height;
			SetEGLContextClientVersion (2);
			shellRenderer = new ShellRenderer ();
			SetRenderer (shellRenderer);
		}
	}
}

