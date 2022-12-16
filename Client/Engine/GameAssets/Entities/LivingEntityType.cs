using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoxelEngine.Engine.GameAssets.Entities {
	class LivingEntityType : EntityType {
		public byte maxHealth;

		public LivingEntityType(string name, byte maxHealth, Vector3 collider_offset, Vector3 collider_size) : base(name, collider_offset, collider_size) {
			this.maxHealth = maxHealth;
		}
	}
}
