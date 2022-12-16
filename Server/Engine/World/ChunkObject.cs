using System.Collections.Generic;
using System.Numerics;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.Networking;

namespace VoxelEngine.Engine.World {
	class ChunkObject {
		public VoxelObject[,,] voxels { private set; get; }
		public Vector3 CHUNK_POS;
		public Vector3 CHUNK_WORLD_POS {
			get {
				return CHUNK_POS.Mult(ChunkManager.CHUNK_SIZE.vector);
			}
		}

		public ChunkObject(Vector3 cHUNK_POS) {
			CHUNK_POS = cHUNK_POS;
		}

		public void GenerateVoxels(VoxelObject[] v = null) {
			voxels = new VoxelObject[ChunkManager.CHUNK_SIZE.x, ChunkManager.CHUNK_SIZE.y, ChunkManager.CHUNK_SIZE.z];
			if (v == null) {
				for (int x = 0; x < ChunkManager.CHUNK_SIZE.x; x++) {
					for (int y = 0; y < ChunkManager.CHUNK_SIZE.y; y++) {
						for (int z = 0; z < ChunkManager.CHUNK_SIZE.z; z++) {
							voxels[x, y, z] = WorldGenerator.GenerateVoxel(new Vector3(x, y, z) + CHUNK_WORLD_POS);
						}
					}
				}
			}
			else {
				for (int x = 0; x < ChunkManager.CHUNK_SIZE.x; x++) {
					for (int y = 0; y < ChunkManager.CHUNK_SIZE.y; y++) {
						for (int z = 0; z < ChunkManager.CHUNK_SIZE.z; z++) {
							voxels[x, y, z] = v[z + (y * ChunkManager.CHUNK_SIZE.z) + (x * ChunkManager.CHUNK_SIZE.z * ChunkManager.CHUNK_SIZE.y)];
						}
					}
				}
			}
		}
	}
}
