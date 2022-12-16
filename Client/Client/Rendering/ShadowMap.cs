using System.Numerics;
using VoxelEngine.Engine.Misc;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering {
	class ShadowMap {
		public static ShadowMap m_instance;

		private ushort SHADOWMAP_SIZE;
		private uint FBO;
		private float ORTHOGRAPHIC_LENGTH;
		private Matrix4x4 ORTHOGRAPHIC_PROJECTION;

		public Shader SHADOWMAP_SHADER { private set; get; }
		public uint SHADOWMAP_TEXTURE { private set; get; }
		public int SHADOWMAP_TEXTURE_UNIT { private set; get; }


		public unsafe ShadowMap(ushort sHADOWMAP_SIZE, ushort oRTHOGRAPHIC_SIZE) {
			if (m_instance != null) ConOut.Warn("There's more than one shadow map!");
			m_instance = this;

			SHADOWMAP_SIZE = sHADOWMAP_SIZE;
			SHADOWMAP_TEXTURE_UNIT = 2;

			SHADOWMAP_TEXTURE = glGenTexture();
			glBindTexture(GL_TEXTURE_2D, SHADOWMAP_TEXTURE);
			glTexImage2D(GL_TEXTURE_2D, 0, GL_DEPTH_COMPONENT, SHADOWMAP_SIZE, SHADOWMAP_SIZE, 0, GL_DEPTH_COMPONENT, GL_FLOAT, null);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER);
			float[] clampColor = { 1.0f, 1.0f, 1.0f, 1.0f };
			glTexParameterfv(GL_TEXTURE_2D, GL_TEXTURE_BORDER_COLOR, clampColor);

			FBO = glGenFramebuffer();
			glBindFramebuffer(GL_FRAMEBUFFER, FBO);
			glFramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_TEXTURE_2D, SHADOWMAP_TEXTURE, 0);
			glDrawBuffer(GL_NONE);
			glReadBuffer(GL_NONE);
			glBindFramebuffer(GL_FRAMEBUFFER, 0);

			ORTHOGRAPHIC_LENGTH = 200f;
			ORTHOGRAPHIC_PROJECTION = Matrix4x4.CreateOrthographic(
				oRTHOGRAPHIC_SIZE, oRTHOGRAPHIC_SIZE,
				1f, ORTHOGRAPHIC_LENGTH
			);

			SHADOWMAP_SHADER = Shader.CreateFromFiles(FileManager.ResourcesPath + "Shaders/shadowMap.frag", FileManager.ResourcesPath + "Shaders/shadowMap.vert");
		}

		public Matrix4x4 GetLightProjection() {
			Vector3 lightCenter = Camera.MainCamera.position.Floor();
			Matrix4x4 lightView = Matrix4x4.CreateLookAt(
				lightCenter - (DirectionalLight.m_instance.DIRECTION * ORTHOGRAPHIC_LENGTH * 0.8f),
				lightCenter,
				Vector3.UnitY
			);
			return lightView * ORTHOGRAPHIC_PROJECTION;
		}

		public void PreRender() {
			glEnable(GL_DEPTH_TEST);
			glViewport(0, 0, SHADOWMAP_SIZE, SHADOWMAP_SIZE);
			glBindFramebuffer(GL_FRAMEBUFFER, FBO);

			glClear(GL_DEPTH_BUFFER_BIT);
		}

		public void PostRender() {
			glActiveTexture(GL_TEXTURE0);
			glBindFramebuffer(GL_FRAMEBUFFER, 0);

			glActiveTexture(GL_TEXTURE0 + SHADOWMAP_TEXTURE_UNIT);
			glBindTexture(GL_TEXTURE_2D, SHADOWMAP_TEXTURE);
		}

		public void Delete() {
			glDeleteFramebuffer(FBO);
			glDeleteTexture(SHADOWMAP_TEXTURE);
			SHADOWMAP_SHADER.Delete();
		}
	}
}
