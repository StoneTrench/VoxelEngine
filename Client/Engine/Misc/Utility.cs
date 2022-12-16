using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace VoxelEngine.Engine.Misc {
	static class Utility {

		public static float[] GetMatrix4x4Values(Matrix4x4 m) {
			return new float[] {
				m.M11, m.M12, m.M13, m.M14,
				m.M21, m.M22, m.M23, m.M24,
				m.M31, m.M32, m.M33, m.M34,
				m.M41, m.M42, m.M43, m.M44
			};
		}

		public static string ArrayToString(IEnumerable enumerable, int truncateTo = -1, string separator = ", ") {
			List<string> result = new List<string>();

			foreach (var item in enumerable) {
				if (truncateTo > -1 && result.Count > truncateTo) {
					result.Add(item.ToString() + "...");
					break;
				}
				else
					result.Add(item.ToString());
			}

			return string.Join(separator, result);
		}

		public static float Clamp(float value, float min, float max) {
			return value > max ? max : value < min ? min : value;
		}

		public static byte ClampB(int n) {
			n &= -(n >= 0 ? 1 : 0);
			return (byte)(n | ((255 - n) >> 31));
		}

		public static Vector3 Floor(this Vector3 vector) {
			return new Vector3(MathF.Floor(vector.X), MathF.Floor(vector.Y), MathF.Floor(vector.Z));
		}
		public static Vector2 Floor(this Vector2 vector) {
			return new Vector2(MathF.Floor(vector.X), MathF.Floor(vector.Y));
		}
		public static Vector2 Abs(this Vector2 vector) {
			return new Vector2(MathF.Abs(vector.X), MathF.Abs(vector.Y));
		}
		public static Vector3 Abs(this Vector3 vector) {
			return new Vector3(MathF.Abs(vector.X), MathF.Abs(vector.Y), MathF.Abs(vector.Z));
		}

		public static Vector3 Offset(this Vector3 vector, float x, float y, float z) {
			return vector + new Vector3(x, y, z);
		}
		public static Vector3 Mult(this Vector3 vector, Vector3 other) {
			return new Vector3(vector.X * other.X, vector.Y * other.Y, vector.Z * other.Z);
		}
		public static Vector3 Divide(this Vector3 vector, Vector3 other) {
			return new Vector3(vector.X / other.X, vector.Y / other.Y, vector.Z / other.Z);
		}
	}
}
