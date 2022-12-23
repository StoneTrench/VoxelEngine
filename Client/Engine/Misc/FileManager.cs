using System;

namespace VoxelEngine.Engine.Misc {
	static class FileManager {
		public static readonly string ResourcesPath = string.Format("{0}/{1}", AppDomain.CurrentDomain.BaseDirectory, "Resources/");
	}
}