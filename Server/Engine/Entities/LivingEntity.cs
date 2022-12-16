using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Engine.Entities {
	class LivingEntity : Entity {
		public byte health;

		public override EntityData entityData {
			get {
				return new LivingEntityData(base.entityData, health);
			}
			set {
				base.entityData = value;
				health = (value as LivingEntityData).health;
			}
		}

		public LivingEntity(LivingEntityData entity) : base(entity) {
			health = entity.health;
		}

		public override void Update(ChunkManager chunkManager) {
			base.Update(chunkManager);
		}
	
		public virtual void DealDamage(Entity source, byte damage) {
			if (health <= damage) {
				health = 0;
				Kill(source);
				return;
			}
			health -= damage;
		}
	}

	[Serializable]
	class LivingEntityData : EntityData {
		public byte health;

		public LivingEntityData(EntityData entityData, byte health) : base(entityData) {
			this.health = health;
		}
	}
}
