using System.Drawing;
using System.Numerics;
using VoxelEngine.Engine.Misc;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering {
	class Skybox {
		private float[] SKYBOX_VERICES = {
			-1.0f, -1.0f,  1.0f,//        7--------6
			 1.0f, -1.0f,  1.0f,//       /|       /|
			 1.0f, -1.0f, -1.0f,//      4--------5 |
			-1.0f, -1.0f, -1.0f,//      | |      | |
			-1.0f,  1.0f,  1.0f,//      | 3------|-2
			 1.0f,  1.0f,  1.0f,//      |/       |/
			 1.0f,  1.0f, -1.0f,//      0--------1
			-1.0f,  1.0f, -1.0f
		};
		private int[] SKYBOX_INDICES = {
			// Back
			0, 1, 5,
			5, 4, 0,
			// Front
			3, 7, 6,
			6, 2, 3,
			// Top
			4, 5, 6,
			6, 7, 4,
			// Bottom
			0, 3, 2,
			2, 1, 0,
			// Left
			0, 4, 7,
			7, 3, 0,
			// Right
			1, 2, 6,
			6, 5, 1,
		};

		private uint VERTEX_ARRAY_OBJECT;
		private uint VERTEX_BUFFER_OBJECT;
		private uint ELEMENT_BUFFER_OBJECT;
		private Shader SHADER;
		private uint CUBEMAP_TEXTURE;

		public unsafe Skybox(
			string back,
			string front,
			string top,
			string bottom,
			string left,
			string right
		) {
			SHADER = Shader.CreateFromFiles(FileManager.ResourcesPath + "Shaders/skybox.frag", FileManager.ResourcesPath + "Shaders/skybox.vert");

			// Create VAO, VBO, and EBO for the skybox
			VERTEX_ARRAY_OBJECT = glGenVertexArray();
			VERTEX_BUFFER_OBJECT = glGenBuffer();
			ELEMENT_BUFFER_OBJECT = glGenBuffer();

			// Assign data
			glBindVertexArray(VERTEX_ARRAY_OBJECT);
			glBindBuffer(GL_ARRAY_BUFFER, VERTEX_BUFFER_OBJECT);
			fixed (float* ptr = &SKYBOX_VERICES[0])
				glBufferData(GL_ARRAY_BUFFER, SKYBOX_VERICES.Length * sizeof(float), ptr, GL_STATIC_DRAW);
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ELEMENT_BUFFER_OBJECT);
			fixed (int* ptr = &SKYBOX_INDICES[0])
				glBufferData(GL_ELEMENT_ARRAY_BUFFER, SKYBOX_INDICES.Length * sizeof(float), ptr, GL_STATIC_DRAW);
			glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), (void*)0);

			// Cleanup
			glEnableVertexAttribArray(0);
			glBindBuffer(GL_ARRAY_BUFFER, 0);
			glBindVertexArray(0);
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

			string[] facesCubemap = { right, left, top, bottom, front, back };

			// Creates the cubemap texture object
			CUBEMAP_TEXTURE = glGenTexture();
			glBindTexture(GL_TEXTURE_CUBE_MAP, CUBEMAP_TEXTURE);
			glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
			glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
			// These are very important to prevent seams
			glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
			glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
			glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);

			for (int i = 0; i < 6; i++) {
				Image image = null;
				try { image = Image.FromFile(facesCubemap[i]); } catch (System.Exception e) { ConOut.Error("Error loading texture:\n", e); }

				if (image == null) {
					ConOut.Error("Failed to load texture:", facesCubemap[i]);
					continue;
				}

				image.RotateFlip(RotateFlipType.RotateNoneFlipY);
				byte[] fileData = Texture.ImageToBitmapByteArray(image);
				fixed (byte* ptr = &fileData[0]) {
					glTexImage2D
					(
						GL_TEXTURE_CUBE_MAP_POSITIVE_X + i,
						0,
						GL_SRGB,
						image.Width,
						image.Height,
						0,
						GL_BGR,
						GL_UNSIGNED_BYTE,
						ptr
					);
				}
			}
		}

		public unsafe void Draw() {
			SHADER.UseProgram();
			Camera mainCam = Camera.MainCamera;
			Vector2 windowSize = RenderingHandler.GetWindowSize();

			Matrix4x4 view = Matrix4x4.CreateLookAt(Vector3.Zero, mainCam.forward, Vector3.UnitY);
			Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(mainCam.FOVrad, windowSize.X / windowSize.Y, mainCam.nearPlane, mainCam.farPlane);

			glUniformMatrix4fv(SHADER.GetUniformLocation("u_view"), 1, false, Utility.GetMatrix4x4Values(view));
			glUniformMatrix4fv(SHADER.GetUniformLocation("u_projection"), 1, false, Utility.GetMatrix4x4Values(projection));

			glBindVertexArray(VERTEX_ARRAY_OBJECT);
			glActiveTexture(GL_TEXTURE0);
			glBindTexture(GL_TEXTURE_CUBE_MAP, CUBEMAP_TEXTURE);
			glDrawElements(GL_TRIANGLES, 36, GL_UNSIGNED_INT, (void*)0);
			glBindVertexArray(0);
		}
		public void Delete() {
			SHADER.Delete();
			glDeleteTexture(CUBEMAP_TEXTURE);
		}
	}
}
