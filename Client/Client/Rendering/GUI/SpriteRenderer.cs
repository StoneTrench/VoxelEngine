using System.Numerics;
using VoxelEngine.Engine.Misc;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering.GUI {
	class SpriteRenderer {
		public Vector2 position { private set; get; }
		public Vector2 scale { private set; get; }

		private uint QUAD_VAO, QUAD_VBO;
		private Shader SHADER;
		private Texture TEXTURE;

		public unsafe SpriteRenderer(Texture tEXTURE, Shader sPRITE_SHADER) {
			position = new Vector2(0, 0);
			scale = new Vector2(0, 0);
			QUAD_VAO = glGenVertexArray();
			QUAD_VBO = glGenBuffer();

			glBindVertexArray(QUAD_VAO);
			glBindBuffer(GL_ARRAY_BUFFER, QUAD_VBO);

			fixed (float* ptr = &RenderingHandler.RECT_VERTICES[0])
				glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 24, ptr, GL_STATIC_DRAW);

			glEnableVertexAttribArray(0);
			glVertexAttribPointer(0, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

			glEnableVertexAttribArray(1);
			glVertexAttribPointer(1, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));

			SHADER = sPRITE_SHADER;
			TEXTURE = tEXTURE;
		}
		public SpriteRenderer SetTransform(Vector2 position, Vector2 scale) {
			this.position = position;
			this.scale = scale;
			return this;
		}

		public void Render(Vector2 windowSize, float aspect) {
			glActiveTexture(GL_TEXTURE0);
			glBindTexture(GL_TEXTURE_2D, 0);

			TEXTURE.BindTexture(0);

			SHADER.UseProgram();
			Matrix4x4 model = Matrix4x4.CreateTranslation(new Vector3(this.position, 0f)) * Matrix4x4.CreateScale(new Vector3(this.scale.X, this.scale.Y * aspect, 0f));
			glUniformMatrix4fv(SHADER.GetUniformLocation("u_model"), 1, false, Utility.GetMatrix4x4Values(model));
			glUniform1i(SHADER.GetUniformLocation("u_sprite"), 0);
			glDisable(GL_DEPTH_TEST);

			glBindVertexArray(QUAD_VAO);
			glDrawArrays(GL_TRIANGLES, 0, 6);
		}

		public void Delete() {
			glDeleteVertexArray(QUAD_VAO);
			glDeleteBuffer(QUAD_VBO);
			SHADER.Delete();
			TEXTURE.Delete();
		}
	}
}
