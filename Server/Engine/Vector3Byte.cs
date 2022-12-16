using System.Numerics;
using VoxelEngine.Engine.Misc;

namespace VoxelEngine.Engine {
	struct Vector3Byte {
		public byte x;
		public byte y;
		public byte z;

		public Vector3Byte(byte x, byte y, byte z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public Vector3Byte(int x, int y, int z) {
			this.x = Utility.ClampB(x);
			this.y = Utility.ClampB(y);
			this.z = Utility.ClampB(z);
		}
		public Vector3Byte(Vector3 vector) {
			this.x = Utility.ClampB((int)vector.X);
			this.y = Utility.ClampB((int)vector.Y);
			this.z = Utility.ClampB((int)vector.Z);
		}
		public Vector3 vector {
			get {
				return new Vector3(x, y, z);
			}
		}

		public static Vector3Byte operator -(Vector3Byte a) => new Vector3Byte(-a.x, -a.y, -a.z);
		public static Vector3Byte operator +(Vector3Byte a, Vector3Byte b) => new Vector3Byte(a.x + b.x, a.y + b.y, a.z + b.z);
		public static Vector3Byte operator -(Vector3Byte a, Vector3Byte b) => a + -b;

		public static Vector3Byte operator *(Vector3Byte a, int b) => new Vector3Byte(a.x * b, a.y * b, a.z * b);
		public static Vector3Byte operator *(int b, Vector3Byte a) => a * b;
		public static Vector3Byte operator /(Vector3Byte a, int b) => new Vector3Byte(a.x / b, a.y / b, a.z / b);

		public static bool operator ==(Vector3Byte a, Vector3Byte b) => a.Equals(b);
		public static bool operator !=(Vector3Byte a, Vector3Byte b) => !a.Equals(b);


		public override int GetHashCode() {
			unchecked // Overflow is fine, just wrap
			{
				int hash = (int)2166136261;
				hash = (hash * 16777619) ^ x.GetHashCode();
				hash = (hash * 16777619) ^ y.GetHashCode();
				hash = (hash * 16777619) ^ z.GetHashCode();
				return hash;
			}
		}
		public override bool Equals(object obj) {
			return GetHashCode() == obj.GetHashCode();
		}
		public override string ToString() {
			return $"Vec3Byte({x}, {y}, {z})";
		}
	}
}
