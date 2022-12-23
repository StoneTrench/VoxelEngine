using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;

using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.Networking;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Networking {
	static class ServerHandler {
		public static bool LogPacketSize = false;
		public static int port = 25565;
		public static int maxRenderDistance = 4;

		private static TcpListener server;
		private static bool serverStarted;

		public static List<ServerClient> clients;

		private static object Server_ThreadLock = new object();

		public static int packetCounter { private set; get; }

		public static void Initialize(int port = 25565) {
			packetCounter = 0;

			clients = new List<ServerClient>();

			try {
				server = new TcpListener(System.Net.IPAddress.Any, port);
				server.Start();

				BeginListening();

				serverStarted = true;
			}
			catch (Exception e) {
				ConOut.Error("Initialize:" + e);
				serverStarted = false;
			}
		}

		private static void BeginListening() {
			server.BeginAcceptTcpClient(ClientSocketConnected, server);
		}
		private static void ClientSocketConnected(IAsyncResult ar) {
			try {
				TcpListener listener = ar.AsyncState as TcpListener;
				ServerClient client = new ServerClient(listener.EndAcceptTcpClient(ar));
				clients.Add(client);

				string[] onlineClients = clients.Select(e => e.username).ToArray();

				SendToClients(new Server_ConnetionToServerResponse(onlineClients), clients);
			}
			catch (Exception e) {
				ConOut.Error("ClientSocketConnected:", e);
			}

			BeginListening();
		}
		private static bool IsConnected(ServerClient client) {
			try {
				return client.tcp.Connected;
			}
			catch {
				return false;
			}
		}

		private static void SendToClients(IPacket packet, IEnumerable<ServerClient> cl) {
			lock (Server_ThreadLock) {
				foreach (var client in cl) {
					client.sendToClientQueue.Add(packet);
					packetCounter++;
				}
			}
		}

		public static void NetworkUpdate() {
			if (!serverStarted) return;

			foreach (ServerClient c in clients.ToArray()) {
				if (!IsConnected(c)) {
					c.tcp.Close();
					clients.Remove(c);
					ConOut.Log($"{c.username} disconnected.");
					continue;
				}

				SendToClientDequeue(c);
				RecievedFromClient(c);
			}
		}

		private static void RecievedFromClient(ServerClient client) {
			try {
				if (client.stream.DataAvailable) {
					byte[] data = new byte[PacketHandler.PacketSizeLimit];
					client.stream.Read(data, 0, data.Length);
					PacketHandler.HandlePacket(data, (short)clients.IndexOf(client));
				}
			}
			catch (Exception e) {
				ConOut.Error("RecievedFromClient:", e);
			}
		}
		private static void SendToClientDequeue(ServerClient client) {
			lock (Server_ThreadLock) {
				if (client.Writing || client.sendToClientQueue.Count == 0) return;

				client.Writing = true;
				try {
					Packet_Batch packet_Batch = new Packet_Batch(new List<IPacket>());
					List<string> sentPacketNames = new List<string>();

					while (client.sendToClientQueue.Count > 0) {
						IPacket packet = client.sendToClientQueue[0];

						if (!packet_Batch.AddPacket(packet)) break;

						sentPacketNames.Add(packet.GetType().Name);

						client.sendToClientQueue.RemoveAt(0);
						packetCounter--;
					}

					if (packet_Batch.size == 0) {
						ConOut.Log(client.sendToClientQueue[0].SerializeBinary().Length);
					}

					client.stream.Write(((IPacket)packet_Batch).SerializeBinary());
					client.stream.Flush();
					if (LogPacketSize) ConOut.Log($"{client.username} Sent packet size: {packet_Batch.size}/{PacketHandler.PacketSizeLimit} B + {Utility.ArrayToString(sentPacketNames)} = {client.sendToClientQueue.Count}");
				}
				catch (Exception e) {
					ConOut.Error("SendToClientsDequeue:", e);
				}
				client.Writing = false;
			}
		}

		public static void CloseSocket() {
			if (!serverStarted) return;

			server.Stop();
			serverStarted = false;
		}


		public static void SendToClients_Exposed(IPacket packet) {
			SendToClients(packet, clients);
		}
		public static void SendToClients_Exposed(IPacket packet, ServerClient client) {
			SendToClients(packet, new ServerClient[] { client });
		}
		public static void SendToClients_Exposed_WhoCanSee(IPacket packet, Vector3 world_pos) {
			SendToClients(packet, FilterByRenderDistance(world_pos));

		}

		public static IEnumerable<ServerClient> FilterByRenderDistance(Vector3 world_pos) {
			return clients.Where(e => ChunkManager.InRenderDistance(e.chunk_pos, e.renderDistance, world_pos));
		}
	}
	class ServerClient {
		public string username;
		public TcpClient tcp;
		public NetworkStream stream;
		public bool Writing;
		public List<IPacket> sendToClientQueue;

		public Vector3 chunk_pos;
		public byte renderDistance;
		public uint entityUUID;

		public ServerClient(TcpClient tcp) {
			this.username = null;
			this.Writing = false;
			this.tcp = tcp;
			this.stream = tcp.GetStream();
			this.sendToClientQueue = new List<IPacket>();
		}
	}
}
