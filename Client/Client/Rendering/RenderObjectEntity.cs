using System.Numerics;

namespace VoxelEngine.Client.Rendering {
	class RenderObjectEntity {
		private EntityPart[] parts;

		public RenderObjectEntity(Shader shader, Texture diffuse, Texture specular) {
			parts = new EntityPart[1];
			parts[0] = new EntityPart { model = Matrix4x4.Identity, renderObject = new RenderObject(new Mesh().SetVerticies(VoxelMeshGenerator.CubePrimitive(-Vector3.One / 2, Vector3.One)), shader, diffuse, specular) };
		}

		public void Render(Matrix4x4 model, bool isShadow) {
			foreach (var item in parts) {
				item.renderObject.Render(item.model * model, isShadow);
			}
		}

		public void Delete() {
			foreach (var item in parts) {
				item.renderObject.Delete();
			}
		}

		struct EntityPart {
			public RenderObject renderObject;
			public Matrix4x4 model;
		}
	}
}
