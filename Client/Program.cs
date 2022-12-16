
using System.Reflection;

namespace VoxelEngine {
    class Program {
        public static readonly string GAME_TITLE = "VoxelEngine";
        public static readonly string GAME_VERSION = "Alpha 0.1";

        public static readonly string GAME_PROJECT_TITLE = Assembly.GetExecutingAssembly().GetName().Name;

        static int Main(string[] args) {
			Client.ClientGameManager gameManager = new Client.ClientGameManager();
			gameManager.Run("client");

			/*ServerGameManager serverGameManager = new ServerGameManager();
			serverGameManager.Run("server");*/

			return 0;
        }
    }
}
