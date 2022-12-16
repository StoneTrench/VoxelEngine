using VoxelEngine.Engine.GameAssets;

namespace VoxelEngine.Engine.World {
	class VoxelObject {
		public ushort VOXEL_ID;

		public VoxelObject(ushort vOXEL_ID) {
			VOXEL_ID = vOXEL_ID;
		}

		public VoxelType VOXEL_TYPE {
			get {
				return GameAssetsHandler.GetVoxel(VOXEL_ID);
			}
		}

		public override string ToString() {
			return $"<{VOXEL_TYPE.name}>";
		}
	}

	[System.Serializable]
	struct SerializableVoxelObject {
		public ushort VOXEL_ID;

		public SerializableVoxelObject(VoxelObject voxel) {
			VOXEL_ID = voxel.VOXEL_ID;
		}
	}
}
