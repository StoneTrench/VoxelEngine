using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

using VoxelEngine.Engine.Entities;
using VoxelEngine.Engine.GameAssets;
using VoxelEngine.Engine.GameAssets.Entities;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.Networking;
using VoxelEngine.Engine.World;
using VoxelEngine.Networking;

namespace VoxelEngine {
	class ServerGameManager : GameManager {

		ChunkManager chunkManager;
		EntityManager entityManager;
		public static byte renderDistanceLimit = 16;
		
		protected override void Initialize() {
			chunkManager = new ChunkManager();
			chunkManager.ChunkVoxelUpdate += (chunk, world_pos) => {
				ServerHandler.SendToClients_Exposed_WhoCanSee(new Server_BlockUpdate(world_pos, chunkManager.GetVoxel(world_pos)), world_pos);
			};

			entityManager = new EntityManager();

			chunkManager.ChunkBulkVoxelUpdate += (chunk, world_poss) => {
				var allVoxels = world_poss.Select(e => chunkManager.GetVoxel(e));

				foreach (var client in ServerHandler.clients) {
					// We filter the voxels for each client so they don't get sent voxels they cant see. And then we put them in a bulk packet.
					var filteredVoxels = allVoxels.Where((e, i) => ChunkManager.InRenderDistance(client.chunk_pos, client.renderDistance, world_poss[i])).ToList();
					var filteredPos = world_poss.Where((e, i) => ChunkManager.InRenderDistance(client.chunk_pos, client.renderDistance, world_poss[i])).ToList();

					ServerHandler.SendToClients_Exposed(new Server_BulkBlockUpdates(filteredPos.ToArray(), filteredVoxels.ToArray()), client);
				}
			};

			RegisterServerHandles();

			ConOut.SetTitle($"{Program.GAME_TITLE} {Program.GAME_VERSION} Server");
			ServerHandler.Initialize(25565);
		}

		private void RegisterServerHandles() {
			PacketHandler.RegisterHandlers(new Dictionary<string, Action<short, IPacket>>() {
				{ "Client_ConnetionToServerRequest", (clientID, packet) => {
					Client_ConnetionToServerRequest p = (Client_ConnetionToServerRequest)packet;
					ServerHandler.clients[clientID].username = p.username;
					SendChatMessage($"{p.username} entered the server");
				} },
				{ "Client_ChatMessage", (clientID, packet) => {
					Client_ChatMessage p = (Client_ChatMessage)packet;

					p.text = $"<{ServerHandler.clients[clientID].username}> {p.text}";
					SendChatMessage(p.text);
				} },
				{ "Client_PlayerPosition", (clientID, packet) => {
					Client_PlayerPosition p = (Client_PlayerPosition)packet;
					byte renderDistance = Math.Min(renderDistanceLimit, p.renderDistance);
					Vector3 player_position = new SerializableVector3(p.position).vector;

					// Recieve player entity
					Entity player_entity = entityManager.GetEntity(ServerHandler.clients[clientID].entityUUID);
					if(player_entity == null)
						ServerHandler.clients[clientID].entityUUID = entityManager.SummonEntity(GameAssetsHandler.GetEntityID("player"), player_position).UUID;
					else player_entity.position = player_position;

					Vector3 player_chunk_pos = ChunkManager.GetChunkPosition(player_position);

					if (player_chunk_pos.Equals(ServerHandler.clients[clientID].chunk_pos) && renderDistance == ServerHandler.clients[clientID].renderDistance) return;

					ServerHandler.clients[clientID].chunk_pos = player_chunk_pos;
					ServerHandler.clients[clientID].renderDistance = renderDistance;

					entityManager.ReassignActiveEntities(ServerHandler.clients.Select(e => e.chunk_pos).ToArray(), ServerHandler.clients.Select(e => e.renderDistance).ToArray());

					// Send chunks
					ChunkManager.ChunkLoopXYZ(renderDistance + 3, (x, y, z) => {
						Vector3 chunk_pos = new Vector3(x, y, z) + player_chunk_pos;
						ChunkObject chunk = chunkManager.GetChunk(chunk_pos);

						if(chunk == null)
							chunk = chunkManager.CreateChunk(chunk_pos);

						if (x >= -renderDistance && y >= -renderDistance && z >= -renderDistance && x < renderDistance && y < renderDistance && z < renderDistance)
							ServerHandler.SendToClients_Exposed(new Server_ChunkData(chunk), ServerHandler.clients[clientID]);
					});
				} },
				{ "Client_BlockBreak", (clientID, packet) => {
					Client_BlockBreak p = (Client_BlockBreak)packet;
					chunkManager.SetVoxel(p.position, GameAssetsHandler.GetVoxelID("air"));
				} },
				{ "Client_BlockPlace", (clientID, packet) => {
					Client_BlockPlace p = (Client_BlockPlace)packet;
					chunkManager.SetVoxel(p.position, p.voxel_id);
				} }
			});
		}

		private void SendChatMessage(string message) {
			ServerHandler.SendToClients_Exposed(new Server_ChatMessage(message));
			ConOut.Log("CHAT:", message);
		}

		/// <summary>
		/// All lists must be the same size.
		/// </summary>
		private List<List<T>>[] SplitArrayToFit<T>(int limit, params List<T>[] lists) {
			List<List<T>>[] result = new List<List<T>>[lists.Length];
			for (int i = 0; i < lists.Length; i++)
				result[i] = new List<List<T>>();

			int size = lists[0].Count;
			float packetCount = (float)size / limit;

			for (int i = 0; i < packetCount; i++) {
				int index = i * limit;
				int count = size - index;

				for (int l = 0; l < lists.Length; l++)
					result[l].Add(lists[l].GetRange(index, limit < count ? limit : count));
			}

			return result;
		}

		protected override void LoadContent() {
			GameAssetsHandler.SetVoxelTypes(
				new VoxelType("unknown", "Unknown", true, true, 0),
				new VoxelType("air", "Air", false, false),
				new VoxelType("stone", "Stone", true, true, 1),
				new VoxelType("dirt", "Dirt", true, true, 3),
				new VoxelType("dirt_grass", "Dirt Grass", true, true, 4, 4, 5, 3, 4, 4),
				new VoxelType("stone_grass", "Stone Grass", true, true, 2, 2, 5, 1, 2, 2),
				new VoxelType("planks", "Planks", true, true, 6)
			);
			GameAssetsHandler.SetEntityTypes(
				new EntityType("unknown", new Vector3(0, 0, 0), new Vector3(1, 1, 1)),
				new EntityType("player", new Vector3(0, 0, 0), new Vector3(0.5f, 1.8f, 0.5f))
			);

			entityManager.SummonEntity(0, new Vector3(0, 64, 0));
		}

		protected override void StartUpdateLoop() {
			while (true) {
				Thread.Sleep(50);

				ServerHandler.NetworkUpdate();
				WorldGenerator.ApplyModifications(chunkManager);

				foreach (var entity in entityManager.ActiveEntities)
					if(entity.ENTITY_TYPE.name != "player")
					entity.Update(chunkManager, 0.01f);

				EntityUpdate();
			}
		}

		
		private int entityPacketTimer = 0;
		private void EntityUpdate() {
			entityPacketTimer++;
			if (entityPacketTimer < 3) return;

			foreach (var client in ServerHandler.clients.ToArray())
				ServerHandler.SendToClients_Exposed(new Server_EntityData(
					entityManager.ActiveEntities.Where(e => ChunkManager.InRenderDistance(client.chunk_pos, client.renderDistance, e.position) && e.UUID != client.entityUUID)
				), client);
			entityPacketTimer = 0;
		}

		protected override void Closing() {
			ServerHandler.CloseSocket();
		}
	}
}
