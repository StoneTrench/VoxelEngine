using System;
using System.Numerics;
using GLFW;
using VoxelEngine.Engine.Misc;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering {
	static class RenderingHandler {
        public static readonly float[] RECT_VERTICES = {
			// Coords    // texCoords
			 1.0f, -1.0f,  1.0f, 0.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
            -1.0f,  1.0f,  0.0f, 1.0f,

             1.0f,  1.0f,  1.0f, 1.0f,
             1.0f, -1.0f,  1.0f, 0.0f,
            -1.0f,  1.0f,  0.0f, 1.0f
        };

        public static Window WINDOW { private set; get; } = Window.None;

        public static event EventHandler<Vector2> WindowResizeEvent;
        public static event EventHandler<uint> ButtonPressedEvent;

        public static void CreateWindow(int width, int height, string title, int swapInterval = 1) {
            if (WINDOW != Window.None) {
                ConOut.Error("Window creation failed. Window already exists");
                return;
			}

            // Prepare context
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);

			// Create window, make the OpenGL context current on the thread, and import graphics functions
            WINDOW = Glfw.CreateWindow(width, height, title, Monitor.None, Window.None);

            if(WINDOW == Window.None) {
                ConOut.Error("Window creation failed");
                return;
			}

            // Center window
            var screen = Glfw.PrimaryMonitor.WorkArea;
            var x = (screen.Width - width) / 2;
            var y = (screen.Height - height) / 2;
            Glfw.SetWindowPosition(WINDOW, x, y);

            Glfw.SetWindowSizeCallback(WINDOW, (_, w, h) => {
                WindowResizeEvent?.Invoke(WINDOW, new Vector2(w, h));
                SetViewPortSize(new Vector2(w, h));
            });
            Glfw.SetCharCallback(WINDOW, (_, codePoint) => ButtonPressedEvent.Invoke(WINDOW, codePoint));

            Glfw.MakeContextCurrent(WINDOW);
            Import(Glfw.GetProcAddress);

            Glfw.SwapInterval(swapInterval);

            ConOut.Log("Window created");
        }
        public static void SetViewPortSize(Vector2 size) {
            glViewport(0, 0, (int)size.X, (int)size.Y);
        }
        public static Vector2 GetWindowSize() {
            Glfw.GetWindowSize(WINDOW, out int width, out int height);
            return new Vector2(width, height);
        }
        public static void SetWindowTitle(string title) {
            Glfw.SetWindowTitle(WINDOW, title);
        }

        public static bool ShouldClose {
            get {
                return Glfw.WindowShouldClose(WINDOW);
			}
		}

        public static void Close() {
            ConOut.Log("Closing...");
            Glfw.DestroyWindow(WINDOW);
            Glfw.Terminate();
		}

        public static Vector2 GetWindowPosition() {
            Glfw.GetWindowPosition(WINDOW, out int x, out int y);
            return new Vector2(x, y);
        }


        public static void SetCursorVisibility(bool visible) {
/*#define GLFW_CURSOR_NORMAL          0x00034001
#define GLFW_CURSOR_HIDDEN          0x00034002
#define GLFW_CURSOR_DISABLED        0x00034003*/
            Glfw.SetInputMode(WINDOW, InputMode.Cursor, visible ? 0x00034001 : 0x00034002);
        }
    }
}
