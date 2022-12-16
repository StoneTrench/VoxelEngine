using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoxelEngine.Engine.GameAssets.Entities {
	class EntityType {
		public string name;

		public Vector3 collider_offset;
		public Vector3 collider_size;

		public EntityType(string name, Vector3 collider_offset, Vector3 collider_size) {
			this.name = name;
			this.collider_offset = collider_offset;
			this.collider_size = collider_size;
		}
	}
}
