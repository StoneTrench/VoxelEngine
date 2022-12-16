using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using VoxelEngine.Engine.GameAssets;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.Noise;

namespace VoxelEngine.Engine.World {
	static class WorldGenerator {
		private static List<StructureVoxel> structureVoxels = new List<StructureVoxel>();
		private static OpenSimplexNoise OpenSimplexNoise = new OpenSimplexNoise(96);

		public static VoxelObject GenerateVoxel(Vector3 world_pos) {
			ushort voxel_id;

			int height = (int)SumHeightMaps(world_pos) + 16;

			if (world_pos.Y < height) {
				if (world_pos.Y == height - 1)
					voxel_id = GameAssetsHandler.GetVoxelID("dirt");
				else if (world_pos.Y == height - 2)
					voxel_id = GameAssetsHandler.GetVoxelID("dirt");
				else
					voxel_id = GameAssetsHandler.GetVoxelID("stone");
			}
			else if (world_pos.Y == height) {
				voxel_id = GameAssetsHandler.GetVoxelID("dirt_grass");

				GenerateStructures(world_pos);
			}
			else voxel_id = GameAssetsHandler.GetVoxelID("air");

			return new VoxelObject(voxel_id);
		}
		private static void GenerateStructures(Vector3 world_pos) {
			bool generate = GetNoiseValue(world_pos.X, world_pos.Z, 100, 0) > 0.5f;
			generate = generate && GetNoiseValue(world_pos.X, world_pos.Z, 0.07f, 512.13f) > 0.0f;

			if (!generate) return;

			for (int i = 0; i < 5; i++)
				lock (structureVoxels_threadLock)
					structureVoxels.Add(new StructureVoxel(new Vector3(0, i, 0) + world_pos, GameAssetsHandler.GetVoxelID("planks")));
		}
		private static float SumHeightMaps(Vector3 world_pos) {
			float result = 0;

			for (int i = 0; i < heightMaps.Length; i++) {
				result += heightMaps[i].Calculate2D(world_pos.X, world_pos.Z);
			}

			return result;
		}

		private static object structureVoxels_threadLock = new object();
		private static bool applyingModifications = false;
		public static void ApplyModifications(ChunkManager chunkManager) {
			if (structureVoxels.Count > 0 && !applyingModifications) {
				applyingModifications = true;
				Task.Factory.StartNew(() => {
					try {
						StructureVoxel[] duplicateStructureVoxels;

						lock (structureVoxels_threadLock)
							duplicateStructureVoxels = structureVoxels.ToArray();

						bool[] voxelsPlacementSuccess = chunkManager.SetVoxels(duplicateStructureVoxels);

						List<StructureVoxel> newStructureVoxels = new List<StructureVoxel>();
						for (int i = 0; i < voxelsPlacementSuccess.Length; i++) {
							if (voxelsPlacementSuccess[i]) continue;

							newStructureVoxels.Add(duplicateStructureVoxels[i]);
						}

						lock (structureVoxels_threadLock)
							structureVoxels = newStructureVoxels;

						applyingModifications = false;
					}
					catch (Exception e) {
						ConOut.Error("ApplyModifications:", e);
					}
				});
			}
		}


		public static float GetNoiseValue(float x, float y, float scale, float offset) {
			return (float)OpenSimplexNoise.Evaluate((x + offset) * scale + offset, (y + offset) * scale);
		}


		private static HeightMap[] heightMaps = {
			new HeightMap(0.05f, 0, 8),
			//new HeightMap(0.1f, 1057, 16, 0.5f, 0)
		};
		private struct HeightMap {
			public float main_scale;
			public float main_offset;
			public float main_result_scalar;

			public bool useMask;

			public float mask_scale;
			public float mask_offset;

			public HeightMap(float main_scale, float main_offset, float main_result_scalar, bool useMask, float mask_scale, float mask_offset) : this(main_scale, main_offset, main_result_scalar) {
				this.useMask = useMask;
				this.mask_scale = mask_scale;
				this.mask_offset = mask_offset;
			}

			public HeightMap(float main_scale, float main_offset, float main_result_scalar, float mask_scale, float mask_offset) {
				this.main_scale = main_scale;
				this.main_offset = main_offset;
				this.main_result_scalar = main_result_scalar;
				this.useMask = true;
				this.mask_scale = mask_scale;
				this.mask_offset = mask_offset;
			}
			public HeightMap(float main_scale, float main_offset, float main_result_scalar) {
				this.main_scale = main_scale;
				this.main_offset = main_offset;
				this.main_result_scalar = main_result_scalar;
				this.useMask = false;
				this.mask_scale = 0;
				this.mask_offset = 0;
			}

			public float Calculate2D(float x, float y) {
				float mask = useMask ? GetNoiseValue(x, y, mask_scale, mask_offset) : 1;
				return GetNoiseValue(x, y, main_scale, main_offset) * mask * main_result_scalar + main_result_scalar;
			}
		}
	}

	struct StructureVoxel {
		public Vector3 world_pos;
		public ushort voxel_id;

		public StructureVoxel(Vector3 world_pos, ushort voxel_id) {
			this.world_pos = world_pos;
			this.voxel_id = voxel_id;
		}
	}
}
