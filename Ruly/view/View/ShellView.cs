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

							bindBuffer(shell.Surface, glsl);
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
			var mats = surface.material;

			int max = mats.Count;

			// draw non-alpha material
//			GLES20.GlEnable(GLES20.GlCullFace);
			GLES20.GlEnable (2884);
			GLES20.GlCullFace (GLES20.GlBack);
			GLES20.GlDisable(GLES20.GlBlend);
			for (int r = 0; r < max; r++) {
				Material mat = mats[r];
				drawOneMaterial(glsl, surface, mat);
			}
		}

		private void drawAlpha(ShellSurface miku, GLSL glsl, bool alpha_test) {
			drawNonAlpha (miku, glsl);
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
//			Vector.min(mDifAmb, 1.0f);
			GLES20.GlUniform4fv(glsl.muDif, 1, mDifAmb, 0);

			// draw
			surface.IndexBuffer.Position (mat.face_vert_offset);
			GLES20.GlDrawElements(GLES20.GlTriangles, mat.face_vert_count, GLES20.GlUnsignedShort, surface.IndexBuffer);
			checkGlError("glDrawElements");
		}


		private void bindBuffer(ShellSurface surface, GLSL glsl) {
			GLES20.GlEnableVertexAttribArray(glsl.maPositionHandle);

			GLES20.GlVertexAttribPointer (glsl.maPositionHandle, 3, GLES20.GlFloat, false, 0, surface.VertexBuffer);
			checkGlError("drawGLES20 VertexAttribPointer vertex");
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
			SetEGLContextClientVersion (2);
			shellRenderer = new ShellRenderer ();
			SetRenderer (shellRenderer);
		}
	}
}

