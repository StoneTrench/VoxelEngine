using GLFW;
using System;
using System.Collections.Generic;
using System.Numerics;
using static OpenGL.GL;
using VoxelEngine.Engine.GameAssets;
using VoxelEngine.Engine.Networking;
using VoxelEngine.Engine.World;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Client.Rendering;
using VoxelEngine.Client.Networking;
using VoxelEngine.Engine.GameAssets.Entities;
using System.Linq;
using VoxelEngine.Engine.Physics;

namespace VoxelEngine.Client {
	class ClientGameManager : GameManager {
		private int DEFAULT_WINDOW_WIDTH;
		private int DEFAULT_WINDOW_HEIGHT;
		private string DEFAULT_WINDOW_TITLE;

		ShadowMap ShadowMap;
		FrameBuffer FrameBuffer;
		Skybox skybox;
		Camera camera;
		ClientChunkManager chunkManager;
		ClientEntityManager entityManager;

		float networkUpdateTimer = 0;

		protected override void StartUpdateLoop() {
			byte frameCounter = 0;
			float prevTime = 0;

			while (!RenderingHandler.ShouldClose) {
				ClientTime.Update();

				float timeDiff = ClientTime.TotalElapsedSecondsF - prevTime;
				++frameCounter;
				if (timeDiff >= 1f / 10) {
					ushort FPS = (ushort)MathF.Floor(1 / timeDiff * frameCounter);
					float ms = timeDiff / frameCounter * 1000f;

					RenderingHandler.SetWindowTitle(string.Format("{0} — {1} fps {2} ms delta: {3}", DEFAULT_WINDOW_TITLE, FPS, ms, ClientTime.TimeDeltaF));

					prevTime = ClientTime.TotalElapsedSecondsF;
					frameCounter = 0;
				}

				Update();

				Glfw.PollEvents();

				Render();

				networkUpdateTimer += ClientTime.TimeDeltaF;
				if (networkUpdateTimer > 1f / 20) {
					networkUpdateTimer = 0;
					ClientHandler.NetworkUpdate();
				}
			}
		}

		protected override void Initialize() {
			PacketHandler.RegisterHandlers(new Dictionary<string, Action<short, IPacket>>() {
				{ "Server_ConnetionToServerResponse", (clientID, packet) => {
					Server_ConnetionToServerResponse p = (Server_ConnetionToServerResponse)packet;
					ConOut.Log("Online players:", string.Join(", ", p.onlinePlayers));
				} },
				{ "Server_ChunkData", (clientID, packet) => {
					Server_ChunkData p = (Server_ChunkData)packet;
					chunkManager.chunkManager.RecieveChunk(ConvertPacketToChunk(p));
				} },
				{ "Server_ChatMessage", (clientID, packet) => {
					Server_ChatMessage p = (Server_ChatMessage)packet;
					ConOut.Log("CHAT: " + p.text);
				} },
				{ "Server_BlockUpdate", (clientID, packet) => {
					Server_BlockUpdate p = (Server_BlockUpdate)packet;
					chunkManager.chunkManager.SetVoxel(p.position, p.newVoxel.VOXEL_ID);
				} },
				{ "Server_BulkBlockUpdates", (clientID, packet) => {
					Server_BulkBlockUpdates p = (Server_BulkBlockUpdates)packet;

					StructureVoxel[] voxels = new StructureVoxel[p.newVoxels.Length];
					for (int i = 0; i < p.newVoxels.Length; i++)
						voxels[i] = new StructureVoxel(new Vector3(p.positions_x[i], p.positions_y[i], p.positions_z[i]), p.newVoxels[i]);

					chunkManager.chunkManager.SetVoxels(voxels);
				} },
				{ "Server_EntityData", (clientID, packet) => {
					Server_EntityData p = (Server_EntityData)packet;
					entityManager.entityManager.RecieveEntities(p.entities);
				} }
			});


			DEFAULT_WINDOW_WIDTH = 854;
			DEFAULT_WINDOW_HEIGHT = 480;
			DEFAULT_WINDOW_TITLE = $"{Program.GAME_TITLE} {Program.GAME_VERSION}";

			ConOut.SetTitle(DEFAULT_WINDOW_TITLE);
			RenderingHandler.CreateWindow(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT, DEFAULT_WINDOW_TITLE, 0);

			Random rand = new Random();

			ClientHandler.username = "Player" + rand.Next();
			ClientHandler.ConnectToServer("localhost", 13000);
		}

		private ChunkObject ConvertPacketToChunk(Server_ChunkData chunk) {
			ChunkObject chunkObject = new ChunkObject(new SerializableVector3Int(chunk.chunk_pos).vector);
			VoxelObject[] voxels = new VoxelObject[chunk.voxels.Length];
			for (int i = 0; i < chunk.voxels.Length; i++)
				voxels[i] = new VoxelObject(chunk.palette[chunk.voxels[i]].VOXEL_ID);

			chunkObject.GenerateVoxels(voxels);
			return chunkObject;
		}

		protected override void LoadContent() {
			glEnable(GL_CULL_FACE);
			glCullFace(GL_FRONT);
			glFrontFace(GL_CCW);

			glEnable(GL_DEPTH_TEST);

			GameAssetsHandler.SetVoxelTypes(
				new VoxelType("unknown", "Unknown", true, true, 0),
				new VoxelType("air", "Air", false, false),
				new VoxelType("stone", "Stone", true, true, 1),
				new VoxelType("dirt","Dirt", true, true, 3),
				new VoxelType("dirt_grass","Dirt Grass", true, true, 4, 4, 5, 3, 4, 4),
				new VoxelType("stone_grass","Stone Grass", true, true, 2, 2, 5, 1, 2, 2),
				new VoxelType("planks", "Planks", true, true, 6)
			);
			GameAssetsHandler.SetEntityTypes(
				new EntityType("unknown", new Vector3(0, 0, 0), new Vector3(1, 2, 1))
			);

			new DirectionalLight(-new Vector3(1, 0.5f, -1));

			string texturesPath = FileManager.ResourcesPath + "Textures/Skybox/";
			skybox = new Skybox(
				texturesPath + "back.jpg",
				texturesPath + "front.jpg",
				texturesPath + "top.jpg",
				texturesPath + "bottom.jpg",
				texturesPath + "left.jpg",
				texturesPath + "right.jpg"
			);
			FrameBuffer = new FrameBuffer();
			ShadowMap = new ShadowMap(2048 * 4, 1000);
			chunkManager = new ClientChunkManager();
			entityManager = new ClientEntityManager();

			camera = new Camera(new Vector3(0, 0, 0), 60);
		}

		private Vector3 playerChunkPos = Vector3.UnitY;

		private void Update() {
			camera.TestMovement();
			chunkManager.Update();
			entityManager.Update();

			if (Input.GetMouseButton(MouseButton.Right)) {
				Vector3 forward = camera.forward;
				RayHit hit = PhysicsUtility.CastRay(chunkManager.chunkManager, camera.position, forward);
				if (hit.hasHit) {
					ushort voxel_id = GameAssetsHandler.GetVoxelID("planks");
					//chunkManager.chunkManager.SetVoxel((hit.global_pos - forward).Floor(), voxel_id);
					ClientHandler.SendToServer_Exposed(new Client_BlockPlace((hit.global_pos - forward).Floor(), voxel_id));
				}
			}
			if (Input.GetMouseButton(MouseButton.Left)) {
				RayHit hit = PhysicsUtility.CastRay(chunkManager.chunkManager, camera.position, camera.forward);
				if (hit.hasHit) {
					//chunkManager.chunkManager.SetVoxel(hit.global_pos.Floor(), GameAssetsHandler.GetVoxelID("air"));
					ClientHandler.SendToServer_Exposed(new Client_BlockBreak(hit.global_pos.Floor()));
				}
			}

			Vector3 newPlayerChunkPos = ChunkManager.GetChunkPosition(camera.position);
			if(newPlayerChunkPos != playerChunkPos) {
				playerChunkPos = newPlayerChunkPos;
				ClientHandler.SendToServer_Exposed(new Client_PlayerChunkPosition(playerChunkPos, 4));
			}
		}

		private void Render() {
			ShadowMap.PreRender();

			chunkManager.Render(true);
			entityManager.Render(true);
			#region SetFrameBufferSize
			RenderingHandler.SetViewPortSize(RenderingHandler.GetWindowSize());
			// This would reset the ShadowMap texture binding so do it before the ShadowMap post render
			FrameBuffer.UpdateSize();
			#endregion
			ShadowMap.PostRender();
			FrameBuffer.PreRender();

			glDepthFunc(GL_LEQUAL);
			skybox.Draw();
			glDepthFunc(GL_LESS);

			chunkManager.Render(false);
			entityManager.Render(false);

			FrameBuffer.PostRender();
			Glfw.SwapBuffers(RenderingHandler.WINDOW);
		}

		protected override void Closing() {
			FrameBuffer.Delete();
			ShadowMap.Delete();
			skybox.Delete();

			RenderingHandler.Close();
			ClientHandler.CloseSocket("closing");
		}
	}
}
