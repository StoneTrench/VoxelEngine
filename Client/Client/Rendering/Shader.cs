using VoxelEngine.Engine.Misc;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering {
	class Shader {
		public uint SHADER_PROGRAM { private set; get; }

		public Shader(
			string fragmentCode = @"#version 330 core
									out vec4 result;

                                    uniform vec3 color;

                                    void main()
                                    {
										result = vec4(color, 1.0);
                                    } ",
			string vertexCode = @"#version 330 core
                                  layout (location = 0) in vec3 pos;

                                  void main()
                                  {
									gl_Position = vec4(pos.x, pos.y, pos.z, 1.0);
                                  }"
			) {
			uint fragmentShader = glCreateShader(GL_FRAGMENT_SHADER);
			glShaderSource(fragmentShader, fragmentCode);
			glCompileShader(fragmentShader);
			CheckForShaderCompilerErrors(fragmentShader, "FRAGMENT");

			uint vertexShader = glCreateShader(GL_VERTEX_SHADER);
			glShaderSource(vertexShader, vertexCode);
			glCompileShader(vertexShader);
			CheckForShaderCompilerErrors(fragmentShader, "VERTEX");


			SHADER_PROGRAM = glCreateProgram();
			glAttachShader(SHADER_PROGRAM, fragmentShader);
			glAttachShader(SHADER_PROGRAM, vertexShader);

			glLinkProgram(SHADER_PROGRAM);

			glDeleteShader(fragmentShader);
			glDeleteShader(vertexShader);
		}

		public void UseProgram() {
			glUseProgram(SHADER_PROGRAM);
		}

		public void Delete() {
			glDeleteProgram(SHADER_PROGRAM);
		}

		public int GetUniformLocation(string uniform) {
			return glGetUniformLocation(SHADER_PROGRAM, uniform);
		}

		private void CheckForShaderCompilerErrors(uint shader, string shaderType) {
			int[] status = glGetShaderiv(shader, GL_COMPILE_STATUS, 1);

			if(status[0] == 0) {
				string error = glGetShaderInfoLog(shader);
				ConOut.Error(string.Format("{0} Shader compilation error:\n{1}", shaderType, error));
			}
		}
	
		public static Shader CreateFromFiles(string fragmentFilePath, string vertexFilePath) {
			try {
				return new Shader(System.IO.File.ReadAllText(fragmentFilePath), System.IO.File.ReadAllText(vertexFilePath));
			}
			catch(System.Exception e) {
				ConOut.Error("CreateFromFiles:", e);
				return null;
			}
		}
	}
}
