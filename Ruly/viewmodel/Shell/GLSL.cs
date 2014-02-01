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
	public class GLSL {
		private String TAG = "Ruly.GLSL";

		public int mProgram;

		//// Vertex shader
		// Attributes
		public int maPositionHandle;
		public int maNormalHandle;
		public int maUvHandle;
		public int maBlendHandle;

		// Uniforms
		public int muPMatrix;
		public int muPow;
		public int muMBone;
		public int muLightDir;

		//// Fragment shader
		// texture samplers
		public int msTextureSampler;
		public int msToonSampler;
		public int msSphSampler;
		public int msSpaSampler;

		// Uniforms
		public int muSpaEn;
		public int muSphEn;
		public int muDif;
		public int muSpec;

		public GLSL(String v, String f) {
			mProgram = createProgram(v, f);
			if (mProgram == 0) {
				return;
			}

			GLES20.GlUseProgram(mProgram);
			checkGlError("glUseProgram");

			// attribute & uniform handles
			maPositionHandle	= GLES20.GlGetAttribLocation(mProgram, "aPosition");
			maNormalHandle		= GLES20.GlGetAttribLocation(mProgram, "aNormal");
			maUvHandle			= GLES20.GlGetAttribLocation(mProgram, "aUv");
			maBlendHandle		= GLES20.GlGetAttribLocation(mProgram, "aBlend");
			checkGlError("glGetAttribLocation");

			muPMatrix			= GLES20.GlGetUniformLocation(mProgram, "uPMatrix");
			muPow				= GLES20.GlGetUniformLocation(mProgram, "uPow");
			muMBone				= GLES20.GlGetUniformLocation(mProgram, "uMBone");
			muLightDir			= GLES20.GlGetUniformLocation(mProgram, "uLightDir");

			msTextureSampler	= GLES20.GlGetUniformLocation(mProgram, "sTex");
			msToonSampler		= GLES20.GlGetUniformLocation(mProgram, "sToon");
			msSphSampler		= GLES20.GlGetUniformLocation(mProgram, "sSph");
			msSpaSampler		= GLES20.GlGetUniformLocation(mProgram, "sSpa");

			muSpaEn				= GLES20.GlGetUniformLocation(mProgram, "bSpaEn");
			muSphEn				= GLES20.GlGetUniformLocation(mProgram, "bSphEn");
			muDif				= GLES20.GlGetUniformLocation(mProgram, "uDif");
			muSpec				= GLES20.GlGetUniformLocation(mProgram, "uSpec");
			checkGlError("glGetUniformLocation");
		}

		private int loadShader(int shaderType, String source) {
			int shader = GLES20.GlCreateShader(shaderType);
			if (shader != 0) {
				GLES20.GlShaderSource(shader, source);
				GLES20.GlCompileShader(shader);
				int[] compiled = new int[1];
				GLES20.GlGetShaderiv(shader, GLES20.GlCompileStatus, compiled, 0);
				if (compiled[0] == 0) {
					Log.Error(TAG, "Could not compile shader " + shaderType + ":");
					Log.Error(TAG, GLES20.GlGetShaderInfoLog(shader));
					Log.Error(TAG, "message ends.");
					GLES20.GlDeleteShader(shader);
					shader = 0;
				}
			}
			return shader;
		}

		private int createProgram(String vertexSource, String fragmentSource) {
			int vertexShader = loadShader(GLES20.GlVertexShader, vertexSource);
			if (vertexShader == 0) {
				return 0;
			}

			int pixelShader = loadShader(GLES20.GlFragmentShader, fragmentSource);
			if (pixelShader == 0) {
				return 0;
			}

			int program = GLES20.GlCreateProgram();
			if (program != 0) {
				GLES20.GlAttachShader(program, vertexShader);
				checkGlError("glAttachShader_vs");
				GLES20.GlAttachShader(program, pixelShader);
				checkGlError("glAttachShader_fs");
				GLES20.GlLinkProgram(program);
				int[] linkStatus = new int[1];
				GLES20.GlGetProgramiv(program, GLES20.GlLinkStatus, linkStatus, 0);
				if (linkStatus[0] != GLES20.GlTrue) {
					Log.Error(TAG, "Could not link program: ");
					Log.Error(TAG, GLES20.GlGetProgramInfoLog(program));
					GLES20.GlDeleteProgram(program);
					program = 0;
				}
			}
			return program;
		}

		private void checkGlError(String op) {
			int error;
			while ((error = GLES20.GlGetError()) != GLES20.GlNoError) {
				Log.Error(TAG, op + ": glError " + error + " " + GLU.GluErrorString(error));
			}
		}
	}
}

