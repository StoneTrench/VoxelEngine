using System;
using System.Collections.Generic;
using System.Numerics;
using VoxelEngine.Client.Networking;
using VoxelEngine.Client.Rendering;
using VoxelEngine.Engine.Entities;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.Networking;
using VoxelEngine.Engine.Physics;

namespace VoxelEngine.Client {
	class ClientEntityManager {
		public static ClientEntityManager m_instance;
		public Entity player;

		private Dictionary<uint, RenderObjectEntity> entityRenderObjects;

		public EntityManager entityManager;

		private Shader ENTITY_SHADER;
		private Texture DEBUG_TEXTURE;

		public ClientEntityManager() {
			if (m_instance != null) ConOut.Warn("There's more than one client entity manager!");
			m_instance = this;

			entityRenderObjects = new Dictionary<uint, RenderObjectEntity>();
			entityManager = new EntityManager();
			ENTITY_SHADER = Shader.CreateFromFiles(FileManager.ResourcesPath + "Shaders/default.frag", FileManager.ResourcesPath + "Shaders/default.vert");
			DEBUG_TEXTURE = Texture.CreateFromFile(FileManager.ResourcesPath + "Textures/unknown.png");
		}

		public void Render(bool shadowMap) {
			for (int i = 0; i < entityManager.ActiveEntities.Count; i++) {
				Entity entity = entityManager.ActiveEntities[i];
				entityRenderObjects[entity.UUID].Render(entity.modelMatrix, shadowMap);
			}
		}


		private float speed = 10;
		private float sensitivity = 60;
		public void Update() {
			if (player == null) {
				ConOut.Warn("Player missing!");
				return;
			}

			PlayerMovement();

			player.Update(ClientChunkManager.m_instance.chunkManager, ClientTime.TimeDeltaF);
			ClientHandler.SendToServer_Exposed(new Client_PlayerPosition(player.position, 4));
		}
		public void PlayerMovement() {
			Vector3 facing = Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromYawPitchRoll(player.rotation.Y, 0, 0));

			player.velocity.X = 0;
			player.velocity.Z = 0;

			if (Input.GetButton(GLFW.Keys.W)) {
				player.velocity += speed * facing * ClientTime.TimeDeltaF;
			}
			else if (Input.GetButton(GLFW.Keys.S)) {
				player.velocity += speed * -facing * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.A)) {
				player.velocity += speed * -Vector3.Normalize(Vector3.Cross(facing, Vector3.UnitY)) * ClientTime.TimeDeltaF;
			}
			else if (Input.GetButton(GLFW.Keys.D)) {
				player.velocity += speed * Vector3.Normalize(Vector3.Cross(facing, Vector3.UnitY)) * ClientTime.TimeDeltaF;
			}

			if (Input.GetButton(GLFW.Keys.Space) && player.velocity.Y == 0) {
				player.velocity.Y = 0.1f;
			}
			//if (Input.GetButton(GLFW.Keys.LeftShift)) {
			//	player.velocity += speed * -Vector3.UnitY * ClientTime.TimeDeltaF;
			//}

			Vector2 windowSize = RenderingHandler.GetWindowSize();

			if (RenderingHandler.lockMouse) {
				// When the size has an odd number a 0.5 is left behind on the mouse that causes drift, this removes the first bit. Only allowing even numbers.
				windowSize.X = ((int)windowSize.X) >> 1 << 1;
				windowSize.Y = ((int)windowSize.Y) >> 1 << 1;

				Vector2 mouse = Input.GetCursorPosition();
				Vector2 rot = sensitivity * (mouse - (windowSize / 2)) / windowSize * 0.008f;

				Vector3 newRotation = player.rotation + new Vector3(rot.Y, -rot.X, 0);
				float limits = MathF.PI / 2 - 0.01f;
				newRotation.X = Utility.Clamp(newRotation.X, -limits, limits);
				player.rotation = newRotation;
			}
		}

		public void TestMovement() {
			Vector3 facing = Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromYawPitchRoll(player.rotation.Y, 0, 0));

			if (Input.GetButton(GLFW.Keys.W)) {
				player.position += speed * facing * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.A)) {
				player.position += speed * -Vector3.Normalize(Vector3.Cross(facing, Vector3.UnitY)) * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.S)) {
				player.position += speed * -facing * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.D)) {
				player.position += speed * Vector3.Normalize(Vector3.Cross(facing, Vector3.UnitY)) * ClientTime.TimeDeltaF;
			}

			if (Input.GetButton(GLFW.Keys.Space)) {
				player.position += speed * Vector3.UnitY * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.LeftShift)) {
				player.position += speed * -Vector3.UnitY * ClientTime.TimeDeltaF;
			}

			Vector2 windowSize = RenderingHandler.GetWindowSize();

			if (RenderingHandler.lockMouse) {
				// When the size has an odd number a 0.5 is left behind on the mouse that causes drift, this removes the first bit. Only allowing even numbers.
				windowSize.X = ((int)windowSize.X) >> 1 << 1;
				windowSize.Y = ((int)windowSize.Y) >> 1 << 1;

				Vector2 mouse = Input.GetCursorPosition();
				Vector2 rot = sensitivity * (mouse - (windowSize / 2)) / windowSize * 0.008f;

				Vector3 newRotation = player.rotation + new Vector3(rot.Y, -rot.X, 0);
				float limits = MathF.PI / 2 - 0.01f;
				newRotation.X = Utility.Clamp(newRotation.X, -limits, limits);
				player.rotation = newRotation;
			}
		}

		public void RecieveEntities(IEnumerable<EntityData> entities) {
		List<Entity> result = new List<Entity>();

			foreach (var entity in entities) {
				int index = entityManager.GetActiveEntityIndex(entity.UUID);
				if (index != -1) {
					entityManager.ActiveEntities[index].entityData = entity;
					result.Add(entityManager.ActiveEntities[index]);
				}
				else {
					entityManager.ActiveEntities.Add(new Entity(entity));
					result.Add(entityManager.ActiveEntities[^1]);
				}

				if (!entityRenderObjects.ContainsKey(entity.UUID))
					entityRenderObjects.Add(entity.UUID, new RenderObjectEntity(ENTITY_SHADER, DEBUG_TEXTURE, DEBUG_TEXTURE));
			}
		}
	}
}
