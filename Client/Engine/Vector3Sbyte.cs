using System.Numerics;

namespace VoxelEngine.Engine {
	struct Vector3Sbyte {
		public sbyte x;
		public sbyte y;
		public sbyte z;

		public Vector3Sbyte(sbyte x, sbyte y, sbyte z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}
		public Vector3Sbyte(int x, int y, int z) {
			this.x = (sbyte)x;
			this.y = (sbyte)y;
			this.z = (sbyte)z;
		}
		public Vector3Sbyte(Vector3 vector) {
			this.x = (sbyte)vector.X;
			this.y = (sbyte)vector.Y;
			this.z = (sbyte)vector.Z;
		}
		public Vector3 vector {
			get {
				return new Vector3(x, y, z);
			}
		}

		public static Vector3Sbyte operator -(Vector3Sbyte a) => new Vector3Sbyte(-a.x, -a.y, -a.z);
		public static Vector3Sbyte operator +(Vector3Sbyte a, Vector3Sbyte b) => new Vector3Sbyte(a.x + b.x, a.y + b.y, a.z + b.z);
		public static Vector3Sbyte operator -(Vector3Sbyte a, Vector3Sbyte b) => a + -b;

		public static Vector3Sbyte operator *(Vector3Sbyte a, int b) => new Vector3Sbyte(a.x * b, a.y * b, a.z * b);
		public static Vector3Sbyte operator *(int b, Vector3Sbyte a) => a * b;
		public static Vector3Sbyte operator /(Vector3Sbyte a, int b) => new Vector3Sbyte(a.x / b, a.y / b, a.z / b);

		public static bool operator ==(Vector3Sbyte a, Vector3Sbyte b) => a.Equals(b);
		public static bool operator !=(Vector3Sbyte a, Vector3Sbyte b) => !a.Equals(b);


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
