using System;
using System.Linq;
using System.Numerics;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Engine.Physics {
	struct RayHit {
		public bool hasHit;
		public Vector3 global_pos;

		public RayHit(bool hasHit, Vector3 global_pos) {
			this.hasHit = hasHit;
			this.global_pos = global_pos;
		}
	}

	static class PhysicsUtility {
		public static readonly Vector3[] VoxelVertices = new Vector3[8] {
			new Vector3(0, 0, 0),
			new Vector3(1, 0, 0),
			new Vector3(1, 0, 1),
			new Vector3(0, 0, 1),
			new Vector3(0, 1, 0),
			new Vector3(1, 1, 0),
			new Vector3(1, 1, 1),
			new Vector3(0, 1, 1)
		};
		public static readonly Vector3[] faceChecks = new Vector3[6] {
			new Vector3(0, 0, -1),
			new Vector3(0, 0, 1),
			new Vector3(0, 1, 0),
			new Vector3(0, -1, 0),
			new Vector3(-1, 0, 0),
			new Vector3(1, 0, 0)
		};
		public static readonly int[][] VoxelFaces = new int[6][] {
			new int[] { 0, 5, 4, 1 },
			new int[] { 2, 7, 6, 3 },
			new int[] { 4, 6, 7, 5 },
			new int[] { 1, 3, 2, 0 },
			new int[] { 3, 4, 7, 0 },
			new int[] { 1, 6, 5, 2 }
		};

		public static readonly Vector3 gravity = new Vector3(0, -1f, 0);

		/// <summary>
		/// </summary>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="maxDistance"></param>
		/// <returns>Returns a ray object</returns>
		public static RayHit CastRay(ChunkManager chunkManager, Vector3 position, Vector3 direction, float maxDistance = 7) {
			float minStep = 0.15f;
			int stepCount = (int)MathF.Floor(maxDistance / minStep);

			Vector3 currentPos = position;

			for (int i = 0; i < stepCount; i++) {
				if (CheckPointOverlapWithVoxel(chunkManager, currentPos)) return new RayHit(true, currentPos);

				currentPos += direction * minStep;
			}

			return new RayHit(false, currentPos);
		}

		public static bool CheckBoxBoxOverlap(Vector3 a_pos, Vector3 a_size, Vector3 b_pos, Vector3 b_size) {
			float a_max_x = a_pos.X + a_size.X;
			float a_max_Y = a_pos.Y + a_size.Y;
			float a_max_Z = a_pos.Z + a_size.Z;

			float b_max_x = b_pos.X + b_size.X;
			float b_max_Y = b_pos.Y + b_size.Y;
			float b_max_Z = b_pos.Z + b_size.Z;

			return
				a_max_x > b_pos.X && a_pos.X < b_max_x &&
				a_max_Y > b_pos.Y && a_pos.Y < b_max_Y &&
				a_max_Z > b_pos.Z && a_pos.Z < b_max_Z
			;
		}
		public static bool CheckBoxPointOverlap(Vector3 a_pos, Vector3 a_size, Vector3 point) {
			float a_max_x = a_pos.X + a_size.X;
			float a_max_Y = a_pos.Y + a_size.Y;
			float a_max_Z = a_pos.Z + a_size.Z;

			return
				a_max_x > point.X && a_pos.X < point.X &&
				a_max_Y > point.Y && a_pos.Y < point.Y &&
				a_max_Z > point.Z && a_pos.Z < point.Z
			;
		}

		public static Vector3[] GetCornersOfCube(Vector3 position, Vector3 size) {
			return VoxelVertices.Select(e => e.Mult(size) + position).ToArray();
		}
		public static Vector3[] GetFaceOfCube(Vector3 position, Vector3 size, int faceIndex) {
			return VoxelFaces[faceIndex].Select(e => VoxelVertices[e].Mult(size) + position).ToArray();
		}

		public static bool CheckPointOverlapWithVoxel(ChunkManager chunkManager, Vector3 position) {
			VoxelObject voxel = chunkManager.GetVoxel(position.Floor());

			if (voxel == null || !voxel.VOXEL_TYPE.hasCollision) return false;

			return true;
			//return CheckBoxPointOverlap(position.Floor(), Vector3.One, position);
		}
	}
}
