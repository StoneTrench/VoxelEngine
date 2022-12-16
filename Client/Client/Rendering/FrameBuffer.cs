using System.Linq;
using VoxelEngine.Engine.Misc;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering {
	class FrameBuffer {
		private	float[] RECT_VERTICES = {
			// Coords    // texCoords
			 1.0f, -1.0f,  1.0f, 0.0f,
			-1.0f, -1.0f,  0.0f, 0.0f,
			-1.0f,  1.0f,  0.0f, 1.0f,

			 1.0f,  1.0f,  1.0f, 1.0f,
			 1.0f, -1.0f,  1.0f, 0.0f,
			-1.0f,  1.0f,  0.0f, 1.0f
		};

		private uint RECT_VAO, RECT_VBO, FBO, RBO;
		private uint FRAME_BUFFER_TEXTURE;
		private Shader FRAME_BUFFER_SHADER;

		public float GAMMA_CORRECTION = 2.2f;

		public unsafe FrameBuffer() {
			RECT_VAO = glGenVertexArray();
			RECT_VBO = glGenBuffer();
			glBindVertexArray(RECT_VAO);
			glBindBuffer(GL_ARRAY_BUFFER, RECT_VBO);
			fixed(float* ptr = &RECT_VERTICES[0])
				glBufferData(GL_ARRAY_BUFFER, RECT_VERTICES.Length * sizeof(float), ptr, GL_STATIC_DRAW);
			glEnableVertexAttribArray(0);
			glVertexAttribPointer(0, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)0);
			glEnableVertexAttribArray(1);
			glVertexAttribPointer(1, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));

			FBO = glGenFramebuffer();
			glBindFramebuffer(GL_FRAMEBUFFER, FBO);

			var size = RenderingHandler.GetWindowSize();

			FRAME_BUFFER_TEXTURE = glGenTexture();
			glBindTexture(GL_TEXTURE_2D, FRAME_BUFFER_TEXTURE);
			glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, (int)size.X, (int)size.Y, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
			glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, FRAME_BUFFER_TEXTURE, 0);

			RBO = glGenRenderbuffer();
			glBindRenderbuffer(RBO);
			glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, (int)size.X, (int)size.Y);
			glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_RENDERBUFFER, RBO);

			// Error checking framebuffer
			int fboStatus = glCheckFramebufferStatus(GL_FRAMEBUFFER);
			if (fboStatus != GL_FRAMEBUFFER_COMPLETE)
				ConOut.Error("Framebuffer error: " + fboStatus);

			FRAME_BUFFER_SHADER = Shader.CreateFromFiles(FileManager.ResourcesPath + "Shaders/frameBuffer.frag", FileManager.ResourcesPath + "Shaders/frameBuffer.vert");
			FRAME_BUFFER_SHADER.UseProgram();
			glUniform1f(FRAME_BUFFER_SHADER.GetUniformLocation("u_screenTexture"), 0);
		}

		public unsafe void UpdateSize() {
			var size = RenderingHandler.GetWindowSize();

			if (size.X == 0 || size.Y == 0) return;

			glBindRenderbuffer(RBO);
			glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH24_STENCIL8, (int)size.X, (int)size.Y);

			glBindTexture(GL_TEXTURE_2D, FRAME_BUFFER_TEXTURE);
			glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, (int)size.X, (int)size.Y, 0, GL_RGB, GL_UNSIGNED_BYTE, null);
		}

		public void PreRender() {
			glBindFramebuffer(GL_FRAMEBUFFER, FBO);

			glClearColor(0.25f, 0.25f, 0.5f, 1);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
		}

		public void PostRender() {
			glActiveTexture(GL_TEXTURE0);
			glBindFramebuffer(GL_FRAMEBUFFER, 0);

			FRAME_BUFFER_SHADER.UseProgram();
			glUniform1f(FRAME_BUFFER_SHADER.GetUniformLocation("u_gamma"), GAMMA_CORRECTION);
			glBindVertexArray(RECT_VAO);
			glDisable(GL_DEPTH_TEST);

			glBindTexture(GL_TEXTURE_2D, true ? FRAME_BUFFER_TEXTURE : ShadowMap.m_instance.SHADOWMAP_TEXTURE);
			glDrawArrays(GL_TRIANGLES, 0, 6);
		}

		public void Delete() {
			glDeleteBuffer(RBO);
			glDeleteFramebuffer(FBO);

			glDeleteVertexArray(RECT_VAO);
			glDeleteBuffer(RECT_VBO);

			glDeleteTexture(FRAME_BUFFER_TEXTURE);

			FRAME_BUFFER_SHADER.Delete();
		}
	}
}
