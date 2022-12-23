using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Engine.Entities {
	class EntityManager {
		private static uint UUIDCounter = 1;

		// Maps entities into chunks
		private Dictionary<Vector3, List<Entity>> unloadedEntities;
		public List<Entity> ActiveEntities;

		public EntityManager() {
			unloadedEntities = new Dictionary<Vector3, List<Entity>>();
			ActiveEntities = new List<Entity>();
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
	}
}
