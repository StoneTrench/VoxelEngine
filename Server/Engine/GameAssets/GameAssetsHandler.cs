using System;
using VoxelEngine.Engine.GameAssets.Entities;

namespace VoxelEngine.Engine.GameAssets {
	static class GameAssetsHandler {

		private static VoxelType[] voxeltypes;
		private static EntityType[] entitytypes;

		#region Voxels
		public static void SetVoxelTypes(params VoxelType[] voxels) {
			voxeltypes = voxels;
		}

		public static ushort GetVoxelID(string name) {
			int id = Array.FindIndex(voxeltypes, e => e.name == name);
			if (id == -1)
				return 0;
			return (ushort)id;
		}
		public static VoxelType GetVoxel(string name) {
			return voxeltypes[GetVoxelID(name)];
		}
		public static VoxelType GetVoxel(ushort id) {
			if (id < 0 || id >= voxeltypes.Length) return voxeltypes[0];
			return voxeltypes[id];
		}
		#endregion

		#region Entities
		public static void SetEntityTypes(params EntityType[] entities) {
			entitytypes = entities;
		}

		public static ushort GetEntityID(string name) {
			int id = Array.FindIndex(entitytypes, e => e.name == name);
			if (id == -1)
				return 0;
			return (ushort)id;
		}
		public static EntityType GetEntity(string name) {
			return entitytypes[GetVoxelID(name)];
		}
		public static EntityType GetEntity(ushort id) {
			if (id < 0 || id >= entitytypes.Length) return entitytypes[0];
			return entitytypes[id];
		}
		#endregion
	}
}
