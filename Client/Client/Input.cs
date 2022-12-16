using GLFW;
using System;
using System.Collections.Generic;
using System.Numerics;
using VoxelEngine.Client.Rendering;

namespace VoxelEngine.Client {
	static class Input {
        public static Dictionary<string, KeyBinding> keyBindings = new Dictionary<string, KeyBinding>();

        public static bool MOUSE_LOCK_STATE { private set; get; }

        public static void SetCursorPosition(Vector2 position) {
            Glfw.SetCursorPosition(RenderingHandler.WINDOW, position.X, position.Y);;
        }
        public static Vector2 GetCursorPosition() {
            Glfw.GetCursorPosition(RenderingHandler.WINDOW, out double x, out double y);
            return new Vector2((float)x, (float)y);
        }

        public static bool GetButton(Keys key, InputState state = GLFW.InputState.Press) {
            return Glfw.GetKey(RenderingHandler.WINDOW, key) == state;
        }
        public static bool GetMouseButton(MouseButton key, InputState state = GLFW.InputState.Press) {
            return Glfw.GetMouseButton(RenderingHandler.WINDOW, key) == state;
        }

        [Obsolete("Made it to check button clicks. But it does that by default.")]
        public static bool IsMouseInWindow() {
            Vector2 size = RenderingHandler.GetWindowSize();
            Vector2 mouse = GetCursorPosition();
            return mouse.X > -1 && mouse.Y > -1 && mouse.X < size.X && mouse.Y < size.Y;
        }
    }

    struct KeyBinding {
        public string name;
        public Keys keyCode;
        public KeyBinding(string name, Keys keyCode) {
            this.name = name;
			this.keyCode = keyCode;
		}

        public bool GetKey(InputState state = GLFW.InputState.Press) {
            return Input.GetButton(keyCode, state);

        }

		public override int GetHashCode() {
            unchecked {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ name.GetHashCode();
                hash = (hash * 16777619) ^ keyCode.GetHashCode();
                return hash;
            }
        }
		public override bool Equals(object obj) {
            return GetHashCode() == obj.GetHashCode();
        }
		public override string ToString() {
            return $"KeyBind({name}, {keyCode})";
		}
	}
}
