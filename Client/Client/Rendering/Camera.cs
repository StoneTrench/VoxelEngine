using System;
using System.Numerics;
using VoxelEngine.Engine.Entities;
using VoxelEngine.Engine.Misc;

namespace VoxelEngine.Client.Rendering {
	class Camera {
		public static Camera MainCamera;

		public Vector3 position;
		public Vector3 rotation;
		public float FOVrad;

		public float farPlane = 1000.0f;
		public float nearPlane = 0.1f;

		private Entity attachedTo;

		public Camera(Vector3 position, float fOV) {
			this.position = position;
			this.rotation = Vector3.Zero;
			FOVrad = fOV * MathF.PI / 180;
			if (MainCamera == null) MainCamera = this;
		}

		public Matrix4x4 GetProjectionMatrix() {
			Vector2 windowSize = RenderingHandler.GetWindowSize();
			Matrix4x4 view = Matrix4x4.CreateLookAt(position, position + forward, Vector3.UnitY);
			Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(FOVrad, windowSize.X / windowSize.Y, nearPlane, farPlane);
			return view * projection;
		}


		public Vector3 forward {
			get {
				return Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z));
			}
		}

		public void AttachToEntity(Entity entity) {
			attachedTo = entity;
		}

		public void Update() {
			if (attachedTo == null) return;

			position = attachedTo.position;
			rotation = attachedTo.rotation;
		}
	}
}
