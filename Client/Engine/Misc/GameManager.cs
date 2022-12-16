namespace VoxelEngine.Engine.Misc {
	abstract class GameManager {

		public void Run(string name) {
			Initialize();
			ConOut.Log($"Initialized {Program.GAME_TITLE} {name} [{Program.GAME_VERSION}]");

			LoadContent();
			ConOut.Log("Loaded content");

			ConOut.Log("Starting update loop");
			StartUpdateLoop();

			ConOut.Log("Cleanup");
			Closing();
		}

		protected abstract void Initialize();
		protected abstract void Closing();
		protected abstract void StartUpdateLoop();
		protected abstract void LoadContent();
	}
}
