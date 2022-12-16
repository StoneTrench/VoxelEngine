using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using VoxelEngine.Engine;
using VoxelEngine.Engine.GameAssets;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.Physics;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Client {
	static class VoxelMeshGenerator {
		public static readonly Vector2[] voxelUvs = new Vector2[6] {
	new Vector2(0, 0),
	new Vector2(1, 1),
	new Vector2(0, 1),
	new Vector2(0, 0),
	new Vector2(1, 0),
	new Vector2(1, 1)
	};
		
		public static readonly int[,] voxelTris = new int[6, 6] {
	{ 0, 5, 4, 0, 1, 5 },
	{ 2, 7, 6, 2, 3, 7 },
	{ 4, 6, 7, 4, 5, 6 },
	{ 1, 3, 2, 1, 0, 3 },
	{ 3, 4, 7, 3, 0, 4 },
	{ 1, 6, 5, 1, 2, 6 },
	};
		public static readonly float[] faceTint = new float[6] {
			0.75f,
			1f,
			1f,
			0.5f,
			0.5f,
			0.75f
		};

		public static readonly int voxelTextureCount = 16;

		public static float[] GenerateChunkVoxelMesh(ChunkObject chunk) {
			List<float> result = new List<float>();

			for (byte x = 0; x < ChunkManager.CHUNK_SIZE.x; ++x) {
				for (byte y = 0; y < ChunkManager.CHUNK_SIZE.y; ++y) {
					for (byte z = 0; z < ChunkManager.CHUNK_SIZE.z; ++z) {

						Vector3Sbyte position = new Vector3Sbyte(x, y, z);
						VoxelType voxel = GetVoxel(position);

						if (voxel.isVisible) {
							for (int f = 0; f < 6; ++f) {
								VoxelType neighbour = GetVoxel(new Vector3Sbyte(position.vector + PhysicsUtility.faceChecks[f]));
								if (!neighbour.isVisible) {

									for (int v = 0; v < 6; ++v) {
										Vector3 vert = PhysicsUtility.VoxelVertices[voxelTris[f, v]] + position.vector;

										result.Add(vert.X);
										result.Add(vert.Y);
										result.Add(vert.Z);

										float color = faceTint[f];

										result.Add(color);
										result.Add(color);
										result.Add(color);

										result.Add(PhysicsUtility.faceChecks[f].X);
										result.Add(PhysicsUtility.faceChecks[f].Y);
										result.Add(PhysicsUtility.faceChecks[f].Z);

										Vector2 uv = GetUV(v, voxel.textureIndices.Length == 1 ? voxel.textureIndices[0] : voxel.textureIndices[f]);

										result.Add(uv.X);
										result.Add(uv.Y);
									}
								}
							}
						}
					}
				}
			}

			return result.ToArray();

			VoxelType GetVoxel(Vector3Sbyte position) {
					if (position.x < 0 || position.y < 0 || position.z < 0 || position.x >= ChunkManager.CHUNK_SIZE.x || position.y >= ChunkManager.CHUNK_SIZE.y || position.z >= ChunkManager.CHUNK_SIZE.z) {
						VoxelObject voxel = ClientChunkManager.m_instance.chunkManager.GetVoxel(position.vector + chunk.CHUNK_WORLD_POS);
						if (voxel == null) return GameAssetsHandler.GetVoxel(0);
						return voxel.VOXEL_TYPE;
					}
					return chunk.voxels[position.x, position.y, position.z].VOXEL_TYPE;
			}
			Vector2 GetUV(int vertexIndex, int textureIndex) {
				Vector2 uv = new Vector2(
					voxelTextureCount - textureIndex % voxelTextureCount - 1,
					voxelTextureCount - MathF.Floor(textureIndex / voxelTextureCount) - 1
				) + voxelUvs[vertexIndex];

				return uv / voxelTextureCount;
			}
		}

		public static float[] CubePrimitive(Vector3 offset, Vector3 scale) {
			List<float> result = new List<float>();

			for (int f = 0; f < 6; f++) {
				for (int v = 0; v < 6; v++) {
					Vector3 vertex = (PhysicsUtility.VoxelVertices[voxelTris[f, v]] + offset).Mult(scale);
					result.Add(vertex.X);
					result.Add(vertex.Y);
					result.Add(vertex.Z);

					float color = faceTint[f];

					result.Add(color);
					result.Add(color);
					result.Add(color);

					result.Add(PhysicsUtility.faceChecks[f].X);
					result.Add(PhysicsUtility.faceChecks[f].Y);
					result.Add(PhysicsUtility.faceChecks[f].Z);

					Vector2 uv = voxelUvs[v];

					result.Add(uv.X);
					result.Add(uv.Y);
				}
			}

			return result.ToArray();
		}

		/*[Obsolete("Slow, too much memory usage, might bring back for airships.")]
		public static float[] GenerateVoxelMesh(Dictionary<Vector3Byte, VoxelObject> voxels) {
			List<float> result = new List<float>();

			var voxelPositions = voxels.Keys.ToArray();

			for (int i = 0; i < voxelPositions.Length; i++) {

				Vector3Byte position = voxelPositions[i];
				VoxelType voxel = GameAssetsHandler.voxeltypes[GetVoxel(position)];

				if (voxel.isVisible) {
					for (int f = 0; f < faceChecks.Length; f++) {
						VoxelType neighbour = GameAssetsHandler.voxeltypes[GetVoxel(new Vector3Byte(position.vector + faceChecks[f]))];
						if (!neighbour.isVisible) {

							for (int v = 0; v < 6; v++) {
								Vector3 vert = voxelVerts[voxelTris[f, v]] + position.vector;

								result.Add(vert.X);
								result.Add(vert.Y);
								result.Add(vert.Z);

								float color = faceTint[f];

								result.Add(color);
								result.Add(color);
								result.Add(color);

								Vector2 uv = GetUV(v, voxel.textureIndices.Length == 1 ? voxel.textureIndices[0] : voxel.textureIndices[f]);

								result.Add(uv.X);
								result.Add(uv.Y);
							}
						}
					}
				}
			}

			return result.ToArray();

			ushort GetVoxel(Vector3Byte position) {
				if (!voxels.ContainsKey(position)) return 0;
				return voxels[position].VOXEL_ID;
			}

			Vector2 GetUV(int vertexIndex, int textureIndex) {
				Vector2 uv = new Vector2(
					voxelTextureCount - textureIndex % voxelTextureCount - 1,
					voxelTextureCount - MathF.Floor(textureIndex / voxelTextureCount) - 1
				) + voxelUvs[vertexIndex];

				return uv / voxelTextureCount;
			}
		}*/
	}
}
