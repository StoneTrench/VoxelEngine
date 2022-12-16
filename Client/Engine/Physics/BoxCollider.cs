using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Engine.Physics {
	struct BoxCollider {

		public Vector3 offset;
		public Vector3 size;

		public BoxCollider(Vector3 offset, Vector3 size) {
			this.offset = offset;
			this.size = size;
		}

		public bool IsCollidingOnSide(ChunkManager chunkManager, Vector3 position, int face) {
			var faceVerts = PhysicsUtility.GetFaceOfCube(position + offset - (size / 2), size, face);

			for (int vf = 0; vf < faceVerts.Length; vf++) {
				Vector3 testPos = faceVerts[vf] + (PhysicsUtility.faceChecks[face] * 0.1f);
				if (PhysicsUtility.CheckPointOverlapWithVoxel(chunkManager, testPos))
					return true;
			}


			return false;
		}
	}
}
