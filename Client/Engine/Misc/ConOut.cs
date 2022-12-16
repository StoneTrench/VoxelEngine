using System;
using System.Collections;

namespace VoxelEngine.Engine.Misc {
	static class ConOut {
		public static int arrayTruncationLength = 16;

		private static bool enableErrors = true;

		public static void EnableErrors(bool value) {
			enableErrors = value;
		}

		public static void Log(string text) {
			var now = DateTime.Now;
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(string.Format("[{0}:{1}] {2}", now.ToShortTimeString(), now.Second, text));
		}
		public static void Warn(string text) {
			var now = DateTime.Now;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(string.Format("[{0}:{1}] WARNING: {2}", now.ToShortTimeString(), now.Second, text));
		}
		public static void Error(string text) {
			if (!enableErrors) return;
			var now = DateTime.Now;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(string.Format("[{0}:{1}] ERROR: {2}", now.ToShortTimeString(), now.Second, text));
			Console.ForegroundColor = ConsoleColor.White;
		}

		public static void Log(object obj) {
			Log(ToString(obj));
		}
		public static void Warn(object obj) {
			Warn(ToString(obj));
		}
		public static void Error(object obj) {
			Error(ToString(obj));
		}

		public static void Log(params object[] obj) {
			Log(Utility.ArrayToString(obj, -1, " "));
		}
		public static void Warn(params object[] obj) {
			Warn(Utility.ArrayToString(obj, -1, " "));
		}
		public static void Error(params object[] obj) {
			Error(Utility.ArrayToString(obj, -1, " "));
		}

		public static void SetTitle(string text) {
			System.Console.Title = text;
		}

		private static string ToString(object obj) {
			if (obj == null) return "null";
			if (obj.GetType() is IEnumerable) return Utility.ArrayToString(obj as IEnumerable, arrayTruncationLength);
			if (obj.GetType().IsArray) return Utility.ArrayToString(obj as IEnumerable, arrayTruncationLength);
			return obj.ToString();
		}
	}
}
