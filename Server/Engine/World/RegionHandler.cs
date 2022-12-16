using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoxelEngine.Engine.Misc;

namespace VoxelEngine.Engine.World {
	static class RegionHandler {
		public static Vector3Byte REGION_CHUNK_SIZE = new Vector3Byte(8, 8, 8);

		public static Vector3 GetRegionPosition(Vector3 chunk_pos) {
			return chunk_pos.Divide(REGION_CHUNK_SIZE.vector).Floor();
		}
	}
}
