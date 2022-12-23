using System;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Engine.Entities {
	class PlayerEntity : LivingEntity {
		public PlayerEntity(PlayerEntityData entity) : base(entity) {
		}

		public override EntityData entityData {
			get {
				return new PlayerEntityData(base.entityData as PlayerEntityData);
			}
			set {
				base.entityData = value;
			}
		}

		public override void DealDamage(Entity source, byte damage) {
			base.DealDamage(source, damage);
		}

		public override void Kill(Entity source) {
			base.Kill(source);
		}

		public override void Update(ChunkManager chunkManager, float deltaTime) {
			base.Update(chunkManager, deltaTime);
		}
	}

	[Serializable]
	class PlayerEntityData : LivingEntityData {

		public PlayerEntityData(LivingEntityData entityData) : base(entityData, entityData.health) {}
	}
}
