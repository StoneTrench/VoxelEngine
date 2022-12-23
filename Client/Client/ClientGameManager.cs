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
using VoxelEngine.Client.Rendering.GUI;
using VoxelEngine.Engine.Entities;

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
		Canvas canvas;

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

					RenderingHandler.SetWindowTitle(string.Format("{0} — {1} fps {2} ms", DEFAULT_WINDOW_TITLE, FPS, ms));

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
					entityManager.RecieveEntities(p.entities);
				} }
			});


			DEFAULT_WINDOW_WIDTH = 854;
			DEFAULT_WINDOW_HEIGHT = 480;
			DEFAULT_WINDOW_TITLE = $"{Program.GAME_TITLE} {Program.GAME_VERSION}";

			ConOut.SetTitle(DEFAULT_WINDOW_TITLE);
			RenderingHandler.CreateWindow(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT, DEFAULT_WINDOW_TITLE, 0);

			Random rand = new Random();

			ClientHandler.username = "Player" + rand.Next();

			string address = Console.ReadLine();
			int port = address.Contains(":") ? int.Parse(address.Split(":")[1]) : 13000;

			ClientHandler.ConnectToServer(address.Replace(":" + port, ""), port);
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
				new EntityType("unknown", new Vector3(0, 0, 0), new Vector3(1, 1, 1)),
				new EntityType("player", new Vector3(0, 0, 0), new Vector3(0.5f, 1.8f, 0.5f))
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
			ShadowMap = new ShadowMap(1024 * 16, 500);
			chunkManager = new ClientChunkManager();
			entityManager = new ClientEntityManager();
			entityManager.player = new LivingEntity(new LivingEntityData(new EntityData(0, GameAssetsHandler.GetEntityID("player"), new Vector3(0, 64, 0), Vector3.Zero, Vector3.Zero), 20));

			camera = new Camera(new Vector3(0, 0, 0), 60);
			camera.AttachToEntity(entityManager.player);
			canvas = new Canvas();

			canvas.AddTexture(new Vector2(0, 0), "crosshairs.png").SetTransform(Vector2.Zero, new Vector2(10) / DEFAULT_WINDOW_HEIGHT);
		}

		private void Update() {
			RenderingHandler.HandleLockMousePre();

			chunkManager.Update();
			entityManager.Update(); // handles player stuff too
			camera.Update();

			RenderingHandler.HandleLockMousePost();

			if (Input.GetMouseButton(MouseButton.Right)) {
				Vector3 forward = camera.forward;
				RayHit hit = PhysicsUtility.CastRay(chunkManager.chunkManager, camera.position, forward);
				if (hit.hasHit) {
					ushort voxel_id = GameAssetsHandler.GetVoxelID("planks");
					ClientHandler.SendToServer_Exposed(new Client_BlockPlace((hit.global_pos - forward).Floor(), voxel_id));
				}
			}
			if (Input.GetMouseButton(MouseButton.Left)) {
				RayHit hit = PhysicsUtility.CastRay(chunkManager.chunkManager, camera.position, camera.forward);
				if (hit.hasHit) {
					ClientHandler.SendToServer_Exposed(new Client_BlockBreak(hit.global_pos.Floor()));
				}
			}
		}

		private void Render() {
			ShadowMap.PreRender();

			chunkManager.Render(true);
			entityManager.Render(true);
			#region SetFrameBufferSize
			RenderingHandler.SetViewPortSize(RenderingHandler.GetWindowSize());
			// This would reset the ShadowMap texture binding so do it before the ShadowMap post render
			#endregion
			ShadowMap.PostRender();
			FrameBuffer.PreRender();

			glDepthFunc(GL_LEQUAL);
			skybox.Draw();
			glDepthFunc(GL_LESS);

			chunkManager.Render(false);
			entityManager.Render(false);
			canvas.Render();

			FrameBuffer.PostRender();
			Glfw.SwapBuffers(RenderingHandler.WINDOW);
		}

		protected override void Closing() {
			FrameBuffer.Delete();
			ShadowMap.Delete();
			skybox.Delete();
			canvas.Delete();

			RenderingHandler.Close();
			ClientHandler.CloseSocket("closing");
		}
	}
}
