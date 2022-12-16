using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoxelEngine.Engine.Networking {
	[Serializable]
	struct SerializableVector3Int {
		public int X;
		public int Y;
		public int Z;

		public Vector3 vector {
			get {
				return new Vector3(X, Y, Z);
			}
		}
		public int[] array {
			get {
				return new int[] { X, Y, Z };
			}
		}

		public SerializableVector3Int(Vector3 vector) {
			X = (int)vector.X;
			Y = (int)vector.Y;
			Z = (int)vector.Z;
		}

		public SerializableVector3Int(int x, int y, int z) {
			X = x;
			Y = y;
			Z = z;
		}
		public SerializableVector3Int(int[] array) {
			X = array[0];
			Y = array[1];
			Z = array[2];
		}
	}
}
