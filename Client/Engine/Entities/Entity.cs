using System;
using System.Numerics;
using VoxelEngine.Engine.GameAssets.Entities;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.Networking;
using VoxelEngine.Engine.Physics;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Engine.Entities {
	class Entity {
		public readonly uint UUID;

		public ushort ENTITY_ID;

		public Vector3 position;
		public Vector3 rotation;
		public Vector3 velocity;

		protected BoxCollider collider;

		public event EventHandler<Entity> onDeath;

		public virtual EntityData entityData {
			get {
				return new EntityData(UUID, ENTITY_ID, position, rotation, velocity);
			}
			set {
				ENTITY_ID = value.ENTITY_ID;
				position = new SerializableVector3(value.position).vector;
				rotation = new SerializableVector3(value.rotation).vector;
				velocity = new SerializableVector3(value.velocity).vector;

				EntityType type = GameAssets.GameAssetsHandler.GetEntity(value.ENTITY_ID);

				collider = new BoxCollider(type.collider_offset, type.collider_size);
			}
		}
		public Matrix4x4 modelMatrix {
			get {
				return Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix4x4.CreateTranslation(position);
			}
		}

		public Entity(EntityData entity) {
			UUID = entity.UUID;
			entityData = entity;
		}

		public EntityType ENTITY_TYPE {
			get {
				return GameAssets.GameAssetsHandler.GetEntity(ENTITY_ID);
			}
		}
	
		public virtual void Update(ChunkManager chunkManager) {
			if (chunkManager.GetChunkFromWorldPos(position) == null) return;

			velocity += PhysicsUtility.gravity;

			HandleCollision(chunkManager);

			position += velocity;
		}

		private void HandleCollision(ChunkManager chunkManager) {
			Vector3 anticipatedPosition = position + (velocity / 2);

			if (collider.IsCollidingOnSide(chunkManager, anticipatedPosition, 0) && velocity.Z < 0) // back
				velocity.Z = 0;
			if (collider.IsCollidingOnSide(chunkManager, anticipatedPosition, 1) && velocity.Z > 0) // front
				velocity.Z = 0;

			if (collider.IsCollidingOnSide(chunkManager, anticipatedPosition, 2) && velocity.Y > 0) // top
				velocity.Y = 0;
			if (collider.IsCollidingOnSide(chunkManager, anticipatedPosition, 3) && velocity.Y < 0) // bottom
				velocity.Y = 0;

			if (collider.IsCollidingOnSide(chunkManager, anticipatedPosition, 4) && velocity.X < 0) // left
				velocity.X = 0;
			if (collider.IsCollidingOnSide(chunkManager, anticipatedPosition, 5) && velocity.X > 0) // right
				velocity.X = 0;
		}

		public virtual void Kill(Entity source) {
			onDeath?.Invoke(this, source);
		}
	}

	[Serializable]
	class EntityData {
		public uint UUID;
		public ushort ENTITY_ID;

		public float[] position;
		public float[] rotation;
		public float[] velocity;

		public EntityData(uint uUID, ushort eNTITY_ID, Vector3 position, Vector3 rotation, Vector3 velocity) {
			UUID = uUID;
			ENTITY_ID = eNTITY_ID;
			this.position = new SerializableVector3(position).array;
			this.rotation = new SerializableVector3(rotation).array;
			this.velocity = new SerializableVector3(velocity).array;
		}
		protected EntityData(EntityData data) {
			UUID = data.UUID;
			ENTITY_ID = data.ENTITY_ID;
			position = new SerializableVector3(data.position).array;
			rotation = new SerializableVector3(data.rotation).array;
			velocity = new SerializableVector3(data.velocity).array;
		}
	}
}
