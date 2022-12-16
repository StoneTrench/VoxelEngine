using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoxelEngine.Engine.Networking {
	[Serializable]
	struct SerializableVector3 {
		public float X;
		public float Y;
		public float Z;

		public Vector3 vector {
			get {
				return new Vector3(X, Y, Z);
			}
		}
		public float[] array {
			get {
				return new float[] { X, Y, Z };
			}
		}

		public SerializableVector3(Vector3 vector) {
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
		}

		public SerializableVector3(float x, float y, float z) {
			X = x;
			Y = y;
			Z = z;
		}
		public SerializableVector3(float[] array) {
			X = array[0];
			Y = array[1];
			Z = array[2];
		}
	}
}
