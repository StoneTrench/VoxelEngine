using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Engine.Entities {
	class EntityManager {
		private static uint UUIDCounter = 0;

		// Maps entities into chunks
		private Dictionary<Vector3, List<Entity>> unloadedEntities;
		public List<Entity> ActiveEntities;

		#region Events

		public event EventHandler<IEnumerable<Entity>> RecievedEntities;

		#endregion

		public EntityManager() {
			unloadedEntities = new Dictionary<Vector3, List<Entity>>();
			ActiveEntities = new List<Entity>();
		}

		public IEnumerable<Entity> GetVisibleEntities(Vector3 chunk_pos, byte renderDistance) {
			return ActiveEntities.Where(e => ChunkManager.InRenderDistance(chunk_pos, renderDistance, e.position));
		}
		public void ReassignActiveEntities(Vector3[] chunk_poss, byte[] renderDistances) {
			foreach (var active in ActiveEntities) {
				Vector3 entity_chunk_pos = ChunkManager.GetChunkPosition(active.position);

				if (unloadedEntities.ContainsKey(entity_chunk_pos))
					unloadedEntities[entity_chunk_pos].Add(active);
				else
					unloadedEntities.Add(entity_chunk_pos, new List<Entity>() { active });
			}

			ActiveEntities.Clear();

			for (int i = 0; i < chunk_poss.Length; i++) {
				ChunkManager.ChunkLoopXYZ(renderDistances[i], (x, y, z) => {
					Vector3 chunk_pos = new Vector3(x, y, z) + chunk_poss[i];
					if (!unloadedEntities.ContainsKey(chunk_pos)) return;

					ActiveEntities.AddRange(unloadedEntities[chunk_pos]);
					unloadedEntities[chunk_pos].Clear();
				});
			}
		}

		public int GetActiveEntityIndex(uint UUID) {
			return ActiveEntities.FindIndex(e => e.UUID == UUID);
		}
		public Entity GetEntity(uint UUID) {
			return ActiveEntities.Find(e => e.UUID == UUID);
		}
		public IEnumerable<Entity> GetEntities(uint[] UUID) {
			return ActiveEntities.Where(e => UUID.Contains(e.UUID));
		}

		public static uint GenerateUUID() {
			return UUIDCounter++;
		}
		public Entity SummonEntity(ushort entity_id, Vector3 position) {
			ActiveEntities.Add(new Entity(new EntityData(GenerateUUID(), entity_id, position, Vector3.Zero, Vector3.Zero)));
			return ActiveEntities[^1];
		}
	
		public void RecieveEntities(IEnumerable<EntityData> entities) {
			List<Entity> result = new List<Entity>();

			foreach (var entity in entities) {
				int index = GetActiveEntityIndex(entity.UUID);
				if (index != -1) {
					ActiveEntities[index].entityData = entity;
					result.Add(ActiveEntities[index]);
				}
				else {
					ActiveEntities.Add(new Entity(entity));
					result.Add(ActiveEntities[^1]);
				}
			}

			RecievedEntities?.Invoke(this, result);
		}
	}
}
