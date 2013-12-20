using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Opengl;
using Android.Util;

namespace Ruly.viewmodel
{
	public class RenderTarget
	{
		private String TAG = "Ruly.RenderTarget";

		private int FBO;
		private int RBOD;
		private int RBOC;
		private int mWidth;
		private int mHeight;

		public RenderTarget() {
			FBO  = 0;
			RBOD = 0;
			RBOC = 0;
		}

		public RenderTarget(int width, int height) {
			mWidth = width;
			mHeight = height;
			create(width, height);
		}

		public void switchTargetFrameBuffer() {
			GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, FBO);
			if(FBO == 0) {
				GLES20.GlViewport(0, 0, ShellViewModel.Width, ShellViewModel.Height);
			} else {
				GLES20.GlViewport(0, 0, mWidth, mHeight);
			}
		}

		public void bindTexture() {
			GLES20.GlBindTexture(GLES20.GlTexture2d, RBOC);
		}

		public void resize(int width, int height) {
			if(FBO != 0) {
				int[] args = {RBOC, RBOD, FBO};
				GLES20.GlDeleteTextures(1, args, 0);
				GLES20.GlDeleteRenderbuffers(1, args, 1);
				GLES20.GlDeleteFramebuffers(1, args, 2);
				create(width, height);
			}
		}

		private void create(int width, int height) {
			// FBO
			int[] ret = new int[1];

			// frame buffer
			GLES20.GlGenFramebuffers(1, ret, 0);
			FBO = ret[0];
			GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, FBO);

			// depth buffer
			GLES20.GlGenRenderbuffers(1, ret, 0);
			RBOD = ret[0];
			GLES20.GlBindRenderbuffer(GLES20.GlFramebuffer, RBOD);
			GLES20.GlRenderbufferStorage(GLES20.GlRenderbuffer, GLES20.GlDepthComponent16, width, height);
			GLES20.GlFramebufferRenderbuffer(GLES20.GlFramebuffer, GLES20.GlDepthAttachment, GLES20.GlRenderbuffer, RBOD);

			// color buffer (is texture)
			GLES20.GlPixelStorei(GLES20.GlUnpackAlignment, 1);
			GLES20.GlGenTextures(1, ret, 0);
			RBOC = ret[0];
			GLES20.GlBindTexture(GLES20.GlTexture2d, RBOC);
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlClampToEdge);
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlClampToEdge);
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlLinear);
			GLES20.GlTexImage2D(GLES20.GlTexture2d, 0, GLES20.GlRgba, width, height, 0, GLES20.GlRgba, GLES20.GlUnsignedByte, null);
			GLES20.GlFramebufferTexture2D(GLES20.GlFramebuffer, GLES20.GlColorAttachment0, GLES20.GlTexture2d, RBOC, 0);

			if(GLES20.GlCheckFramebufferStatus(GLES20.GlFramebuffer) != GLES20.GlFramebufferComplete) {
				Log.Debug(TAG, "Fail to create FBO.");
				FBO = 0;
				RBOD = 0;
				RBOC = 0;
			}
		}
	}
}

