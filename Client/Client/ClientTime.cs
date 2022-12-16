using GLFW;

namespace VoxelEngine.Client {
	static class ClientTime {
		public static double TimeDelta = 0;
		public static double TotalElapsedSeconds = 0;

		public static float TimeDeltaF {
			get {
				return (float)TimeDelta;
			}
		}
		public static float TotalElapsedSecondsF {
			get {
				return (float)TotalElapsedSeconds;
			}
		}

		public static void Update() {
			TimeDelta = Glfw.Time - TotalElapsedSeconds;
			TotalElapsedSeconds = Glfw.Time;
		}
	}
}
