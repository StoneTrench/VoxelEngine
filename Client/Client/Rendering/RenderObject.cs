using System.Numerics;
using VoxelEngine.Client.Rendering;
using VoxelEngine.Engine.Misc;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering {
	class RenderObject {
		public Mesh MESH;
		public Shader SHADER;
		public Texture DIFFUSE_TEXTURE;
		public Texture SPECULAR_TEXTURE;

		public RenderObject(Mesh mESH, Shader sHADER, Texture dIFFUSE_TEXTURE, Texture sPECULAR_TEXTURE) {
			MESH = mESH;
			SHADER = sHADER;
			DIFFUSE_TEXTURE = dIFFUSE_TEXTURE;
			SPECULAR_TEXTURE = sPECULAR_TEXTURE;
		}

		public void Render(Matrix4x4 model, bool shadowMap) {
			Shader shader;
			if (shadowMap) shader = ShadowMap.m_instance.SHADOWMAP_SHADER; 
			else shader = SHADER;

			if (shader != null) {
				shader.UseProgram();
				// Vert uniform
				glUniformMatrix4fv(shader.GetUniformLocation("u_lightProjectionMat"), 1, false, Utility.GetMatrix4x4Values(ShadowMap.m_instance.GetLightProjection()));
				glUniformMatrix4fv(shader.GetUniformLocation("u_cameraMat"), 1, false, Utility.GetMatrix4x4Values(Camera.MainCamera.GetProjectionMatrix()));
				glUniformMatrix4fv(shader.GetUniformLocation("u_modelMat"), 1, false, Utility.GetMatrix4x4Values(model));

				// Frag uniform
				glUniform3f(shader.GetUniformLocation("u_lightPosition"), DirectionalLight.m_instance.DIRECTION.X, DirectionalLight.m_instance.DIRECTION.Y, DirectionalLight.m_instance.DIRECTION.Z);
				glUniform3f(shader.GetUniformLocation("u_cameraPosition"), Camera.MainCamera.position.X, Camera.MainCamera.position.Y, Camera.MainCamera.position.Z);
				glUniform1i(shader.GetUniformLocation("u_diffuseTex"), 0);
				glUniform1i(shader.GetUniformLocation("u_specularTex"), 1);
				glUniform1i(shader.GetUniformLocation("u_shadowMapTex"), ShadowMap.m_instance.SHADOWMAP_TEXTURE_UNIT);
			}

			//glActiveTexture(GL_TEXTURE0 + ShadowMap.MainShadowMap.SHADOWMAP_TEXTURE_UNIT);
			//glBindTexture(GL_TEXTURE_2D, ShadowMap.MainShadowMap.SHADOWMAP_TEXTURE);

			if (DIFFUSE_TEXTURE != null)
				DIFFUSE_TEXTURE.BindTexture(0);
			if (SPECULAR_TEXTURE != null)
				SPECULAR_TEXTURE.BindTexture(1);

			if (MESH != null)
				MESH.Render();
		}
		public void Delete() {
			if (SHADER != null)
				SHADER.Delete();
			if (DIFFUSE_TEXTURE != null)
				DIFFUSE_TEXTURE.Delete();
			if (SPECULAR_TEXTURE != null)
				SPECULAR_TEXTURE.Delete();
			if (MESH != null)
				MESH.Delete();
		}
	}
}
