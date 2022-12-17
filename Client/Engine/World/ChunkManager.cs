using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VoxelEngine.Engine.Misc;

namespace VoxelEngine.Engine.World {
	class ChunkManager {
		public static readonly Vector3Byte CHUNK_SIZE = new Vector3Byte(16, 64, 16);

		public List<Vector3> ActiveChunks { private set; get; }
		private Dictionary<Vector3, ChunkObject> Chunks;

		#region Events

		public event EventHandler<Vector3> ChunkCreated;
		public event EventHandler<Vector3> ChunkRecieved;

		public event EventHandler<Vector3> ChunkVoxelUpdate;
		public event EventHandler<Vector3[]> ChunkBulkVoxelUpdate;

		#endregion

		public ChunkManager() {
			Chunks = new Dictionary<Vector3, ChunkObject>();
			ActiveChunks = new List<Vector3>();
		}

		public ChunkObject CreateChunk(Vector3 chunk_pos) {
			if (Chunks.ContainsKey(chunk_pos)) return null;

			ChunkObject chunk = new ChunkObject(chunk_pos);
			Chunks.Add(chunk_pos, chunk);
			ActiveChunks.Add(chunk_pos);

			chunk.GenerateVoxels();
			ChunkCreated?.Invoke(chunk, chunk_pos);

			return chunk;
		}
		public void RecieveChunk(ChunkObject chunk) {
			if (Chunks.ContainsKey(chunk.CHUNK_POS)) return;

			Chunks.Add(chunk.CHUNK_POS, chunk);

			ActiveChunks.Add(chunk.CHUNK_POS);

			ChunkRecieved?.Invoke(chunk, chunk.CHUNK_POS);
		}

		public static Vector3 GetChunkPosition(Vector3 world_pos) {
			return world_pos.Divide(CHUNK_SIZE.vector).Floor();
		}
		public ChunkObject GetChunkFromWorldPos(Vector3 world_pos) {
			return GetChunk(GetChunkPosition(world_pos));
		}
		public ChunkObject GetChunk(Vector3 chunk_pos) {
			if (!Chunks.ContainsKey(chunk_pos)) return null;
			return Chunks[chunk_pos];
		}
		
		public VoxelObject GetVoxel(Vector3 world_pos) {
			ChunkObject chunk = GetChunkFromWorldPos(world_pos);
			if (chunk == null) return null;
			Vector3Byte local_pos = new Vector3Byte(world_pos - chunk.CHUNK_WORLD_POS);
			return chunk.voxels[local_pos.x, local_pos.y, local_pos.z];
		}
		public bool SetVoxel(Vector3 world_pos, ushort voxel_ID, bool sendEvent = true) {
			ChunkObject chunk = GetChunkFromWorldPos(world_pos);
			if (chunk == null) return false;
			Vector3Byte local_pos = new Vector3Byte(world_pos - chunk.CHUNK_WORLD_POS);
			chunk.voxels[local_pos.x, local_pos.y, local_pos.z] = new VoxelObject(voxel_ID);
			if(sendEvent) ChunkVoxelUpdate?.Invoke(chunk, world_pos);
			return true;
		}
		public bool[] SetVoxels(StructureVoxel[] voxels, bool sendEvent = true) {
			bool[] result = new bool[voxels.Length];

			for (int i = 0; i < voxels.Length; i++)
				result[i] = SetVoxel(voxels[i].world_pos, voxels[i].voxel_id, false);

			if (sendEvent) ChunkBulkVoxelUpdate?.Invoke(this, voxels.Where((e, i) => result[i]).Select(e => e.world_pos).ToArray());

			return result;
		}
		

		public static void ChunkLoopXYZ(int renderDistance, Action<int, int, int> action) {
			for (int x = -renderDistance; x < renderDistance; x++) {
				for (int y = -renderDistance; y < renderDistance; y++) {
					for (int z = -renderDistance; z < renderDistance; z++) {
						action.Invoke(x, y, z);
					}
				}
			}
		}
		public static bool InRenderDistance(Vector3 chunk_pos, byte renderDistance, Vector3 world_pos) {
			Vector3 offsetChunkPos = world_pos.Divide(CHUNK_SIZE.vector) - chunk_pos;
			return offsetChunkPos.X > -renderDistance && offsetChunkPos.Y > -renderDistance && offsetChunkPos.Z > -renderDistance &&
				offsetChunkPos.X < renderDistance && offsetChunkPos.Y < renderDistance && offsetChunkPos.Z < renderDistance;
		}
	}
}
