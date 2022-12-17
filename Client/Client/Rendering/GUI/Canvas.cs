using System.Collections.Generic;
using System.Numerics;
using VoxelEngine.Engine.Misc;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering.GUI {
	class Canvas {
		public List<SpriteRenderer> guiElements = new List<SpriteRenderer>();
		private Shader SPRITE_SHADER;

		public Canvas() {
			SPRITE_SHADER = Shader.CreateFromFiles(FileManager.ResourcesPath + "Shaders/spriteShader.frag", FileManager.ResourcesPath + "Shaders/spriteShader.vert");
		}

		public SpriteRenderer AddTexture(Vector2 position, string texture) {
			guiElements.Add(new SpriteRenderer(new Texture(FileManager.ResourcesPath + "Textures/" + texture), SPRITE_SHADER));
			return guiElements[^1];
		}
		

		public void Render() {
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			Vector2 windowSize = RenderingHandler.GetWindowSize();
			float aspect = windowSize.X / windowSize.Y;
			foreach (var element in guiElements) {
				element.Render(windowSize, aspect);
			}
			glDisable(GL_BLEND);
		}
	}
}
