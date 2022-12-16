using System;
using System.Collections.Generic;
using System.Text;

namespace VoxelEngine.Engine.GameAssets {
	class VoxelType {
		public string name;

		public string displayName;
		public int[] textureIndices;
		public bool isVisible;
		public bool hasCollision;

		public VoxelType(string name, string displayName, bool isVisible, bool hasCollision, params int[] textureIndices) {
			this.name = name;
			this.displayName = displayName;
			this.textureIndices = textureIndices;
			this.isVisible = isVisible;
			this.hasCollision = hasCollision;
		}
	}
}
