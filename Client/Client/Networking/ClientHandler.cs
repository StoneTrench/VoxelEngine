using System.Collections.Generic;
using System.Net.Sockets;
using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.Networking;

namespace VoxelEngine.Client.Networking {
	static class ClientHandler {
		public static bool LogPacketSize = false;
		public static string username = "Player";

        public static bool isConnected = false;

		private static bool socketReady;
		private static TcpClient socket;

		private static bool Writing = false;
		private static NetworkStream stream;

		private static List<IPacket> sendToServerQueue;

		public static bool ConnectToServer(string host, int port) {
			if (socketReady) {
				ConOut.Error("Client already connected to server!");
				return false;
			}

			try {
				sendToServerQueue = new List<IPacket>();
				sendToServerQueue.Add(new Client_ConnetionToServerRequest(username));

				socket = new TcpClient(host, port);
				stream = socket.GetStream();

				socketReady = true;
			}
			catch (System.Exception e) {
				ConOut.Error("ConnectToServer:", e);
			}

			return socketReady;
		}

		public static void CloseSocket(string message) {
			if (!socketReady) return;

			try {
				stream.Close();
			}
			catch (System.Exception e) {
				ConOut.Error("CloseSocketError: " + e);
			}

			socket.Close();

			socketReady = false;
			Writing = false;
		}

		public static void NetworkUpdate() {
			if (!socketReady) return;

			SendToServerDequeue();
			RecievedFromServer();
		}

		private static void RecievedFromServer() {
			try {
				if (stream.DataAvailable) {
					byte[] data = new byte[PacketHandler.PacketSizeLimit];
					stream.Read(data, 0, data.Length);
					PacketHandler.HandlePacket(data);
				}
			}
			catch (System.Exception e) {
				ConOut.Error("RecievedFromServer:", e);
				CloseSocket(e.Message);
			}
		}
		private static void SendToServerDequeue() {
			if (Writing || sendToServerQueue.Count == 0) return;

			Writing = true;
			try {
				Packet_Batch packet_Batch = new Packet_Batch(new List<IPacket>());
				List<string> sentPacketNames = new List<string>();

				while (sendToServerQueue.Count > 0) {
					IPacket packet = sendToServerQueue[0];

					if (!packet_Batch.AddPacket(packet)) break;

					sentPacketNames.Add(packet.GetType().Name);

					sendToServerQueue.RemoveAt(0);
				}

				stream.Write(((IPacket)packet_Batch).SerializeBinary());
				stream.Flush();
				if (LogPacketSize) ConOut.Log($"Sent packet size: {packet_Batch.size}/{PacketHandler.PacketSizeLimit} B + {Utility.ArrayToString(sentPacketNames)} = {sendToServerQueue.Count}");
			}
			catch (System.Exception e) {
				ConOut.Error("SendToServerDequeue:", e);
				CloseSocket(e.Message);
			}
			Writing = false;
		}

		public static void SendToServer_Exposed(IPacket packet) {
			sendToServerQueue.Add(packet);
		}
	}
}
