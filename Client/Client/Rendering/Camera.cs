using System;
using System.Numerics;
using VoxelEngine.Engine.Misc;

namespace VoxelEngine.Client.Rendering {
	class Camera {
		public static Camera MainCamera;

		public Vector3 position;
		public Vector3 rotation;
		public float FOVrad;

		private float speed = 10;
		private float sensitivity = 60;

		public float farPlane = 1000.0f;
		public float nearPlane = 0.1f;

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


		bool lockMouse = false;

		public Vector3 forward {
			get {
				return Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z));
			}
		}

		public void TestMovement() {
			Vector3 facing = Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromYawPitchRoll(rotation.Y, 0, 0));

			if (Input.GetButton(GLFW.Keys.W)) {
				position += speed * facing * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.A)) {
				position += speed * -Vector3.Normalize(Vector3.Cross(facing, Vector3.UnitY)) * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.S)) {
				position += speed * -facing * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.D)) {
				position += speed * Vector3.Normalize(Vector3.Cross(facing, Vector3.UnitY)) * ClientTime.TimeDeltaF;
			}

			if (Input.GetButton(GLFW.Keys.Space)) {
				position += speed * Vector3.UnitY * ClientTime.TimeDeltaF;
			}
			if (Input.GetButton(GLFW.Keys.LeftShift)) {
				position += speed * -Vector3.UnitY * ClientTime.TimeDeltaF;
			}

			Vector2 windowSize = RenderingHandler.GetWindowSize();

			if (!lockMouse && Input.GetMouseButton(GLFW.MouseButton.Left)) {
				lockMouse = true;

				RenderingHandler.SetCursorVisibility(false);
				Input.SetCursorPosition(windowSize / 2);
			}

			if (lockMouse && Input.GetButton(GLFW.Keys.Escape)) {
				lockMouse = false;

				RenderingHandler.SetCursorVisibility(true);
			}

			if (lockMouse) {
				// When the size has an odd number a 0.5 is left behind on the mouse that causes drift, this removes the first bit. Only allowing even numbers.
				windowSize.X = ((int)windowSize.X) >> 1 << 1;
				windowSize.Y = ((int)windowSize.Y) >> 1 << 1;

				Vector2 mouse = Input.GetCursorPosition();
				Vector2 rot = sensitivity * (mouse - (windowSize / 2)) / windowSize * 0.008f;

				Vector3 newRotation = rotation + new Vector3(rot.Y, -rot.X, 0);
				float limits = MathF.PI / 2 - 0.01f;
				newRotation.X = Utility.Clamp(newRotation.X, -limits, limits);
				rotation = newRotation;

				Input.SetCursorPosition(windowSize / 2);
			}
		}
	}
}
