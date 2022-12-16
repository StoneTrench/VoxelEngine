using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering {
	class Mesh {
		private readonly uint VERTEX_ARRAY_OBJECT;
		private readonly uint VERTEX_BUFFER_OBJECT;
		private ushort verticesLength;

		private static readonly byte vertexSize = 3 + 3 + 3 + 2;

		public Mesh() {
			VERTEX_ARRAY_OBJECT = glGenVertexArray();
			VERTEX_BUFFER_OBJECT = glGenBuffer();
		}

		public unsafe Mesh SetVerticies(float[] vertices) {
			verticesLength = (ushort)(vertices.Length / vertexSize);

			if (vertices != null && vertices.Length > 0) {
				glBindVertexArray(VERTEX_ARRAY_OBJECT);
				glBindBuffer(GL_ARRAY_BUFFER, VERTEX_BUFFER_OBJECT);

				fixed (float* ptr = &vertices[0]) {
					glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, ptr, GL_STATIC_DRAW);
				}

				// Position
				glVertexAttribPointer(0, 3, GL_FLOAT, false, vertexSize * sizeof(float), (void*)0);
				glEnableVertexAttribArray(0);

				// Color
				glVertexAttribPointer(1, 3, GL_FLOAT, false, vertexSize * sizeof(float), (void*)(3 * sizeof(float)));
				glEnableVertexAttribArray(1);

				// Normal
				glVertexAttribPointer(2, 3, GL_FLOAT, false, vertexSize * sizeof(float), (void*)(6 * sizeof(float)));
				glEnableVertexAttribArray(2);

				// UV
				glVertexAttribPointer(3, 2, GL_FLOAT, false, vertexSize * sizeof(float), (void*)(9 * sizeof(float)));
				glEnableVertexAttribArray(3);

				glBindBuffer(GL_ARRAY_BUFFER, 0);
				glBindVertexArray(0);
			}
			else
				verticesLength = 0;

			return this;
		}

		public void Render() {
			if (verticesLength > 0) {
				glBindVertexArray(VERTEX_ARRAY_OBJECT);
				glDrawArrays(GL_TRIANGLES, 0, verticesLength);
				glBindVertexArray(0);
			}
		}

		public void Delete() {
			glDeleteVertexArray(VERTEX_ARRAY_OBJECT);
			glDeleteBuffer(VERTEX_BUFFER_OBJECT);
		}
	}
}
