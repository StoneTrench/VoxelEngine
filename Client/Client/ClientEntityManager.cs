using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VoxelEngine.Client.Rendering;
using VoxelEngine.Engine.Entities;
using VoxelEngine.Engine.Misc;

namespace VoxelEngine.Client {
	class ClientEntityManager {
		public static ClientEntityManager m_instance;

		private Dictionary<uint, RenderObject> entityRenderObjects;

		public EntityManager entityManager;

		private Shader ENTITY_SHADER;
		private Texture DEBUG_TEXTURE;

		public ClientEntityManager() {
			if (m_instance != null) ConOut.Warn("There's more than one client entity manager!");
			m_instance = this;

			entityRenderObjects = new Dictionary<uint, RenderObject>();
			entityManager = new EntityManager();
			ENTITY_SHADER = Shader.CreateFromFiles(FileManager.ResourcesPath + "Shaders/default.frag", FileManager.ResourcesPath + "Shaders/default.vert");
			DEBUG_TEXTURE = new Texture(FileManager.ResourcesPath + "Textures/unknown.png");

			entityManager.RecievedEntities += (_, entities) => {
				foreach (var e in entities) {
					if (!entityRenderObjects.ContainsKey(e.UUID))
						entityRenderObjects.Add(e.UUID, new RenderObject(new Mesh().SetVerticies(VoxelMeshGenerator.CubePrimitive(Vector3.One / -2, new Vector3(1, 2, 1))), ENTITY_SHADER, DEBUG_TEXTURE, DEBUG_TEXTURE));
				}
			};
		}

		public void Render(bool shadowMap) {
			for (int i = 0; i < entityManager.ActiveEntities.Count; i++) {
				Entity entity = entityManager.ActiveEntities[i];
				entityRenderObjects[entity.UUID].Render(entity.modelMatrix, shadowMap);
			}
		}
	
		public void Update() {

		}
	}
}
