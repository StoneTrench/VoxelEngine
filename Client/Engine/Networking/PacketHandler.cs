using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections.Generic;

using VoxelEngine.Engine.Misc;
using System.Linq;

namespace VoxelEngine.Engine.Networking {
    static class PacketHandler {
        public const int OneMB = 1024;
        public const int PacketSizeLimit = OneMB * 8;

        private static Dictionary<string, Action<short, IPacket>> _packetHandlers;
        private static readonly BinaryFormatter binaryFormatter = new BinaryFormatter() {
            Binder = new PreMergeToMergedDeserializationBinder(),
            TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesWhenNeeded,
            AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
        };

        //private static int headerByteSize = 0;

        public static void RegisterHandlers(Dictionary<string, Action<short, IPacket>> packetHandlers) {
            //headerByteSize = 23 + Program.GAME_PROJECT_TITLE.Length + 91;

            ConOut.Log("Registering packet handlers");
            // Action is on the side of the reciever
            _packetHandlers = packetHandlers;
            _packetHandlers.Add(
                "Packet_Batch", (clientID, packet) => {
                    Packet_Batch p = (Packet_Batch)packet;
                    foreach (var pac in p.packets) {
                        HandlePacket(pac, clientID);
                    }
                }
            );
        }

        public static void HandlePacket(byte[] packetData, short ClientID = -1) {
            IPacket packet = IPacket.DeserializeBinary(packetData);
            if (packet == null) return;

            string packetID = packet.GetType().Name;

            if (!_packetHandlers.ContainsKey(packetID)) {
                ConOut.Warn($"Packet {packetID} was not registered!");
                return;
            }

            _packetHandlers[packetID].Invoke(ClientID, packet);
        }

        public static byte[] SerializeObject(object obj) {
            try {
                if (obj == null)
                    return null;

                using MemoryStream memoryStream = new MemoryStream();
                binaryFormatter.Serialize(memoryStream, obj);

                return Compress(memoryStream.ToArray());
            }
            catch (System.Exception e) {
                ConOut.Error("SerializeObject:", e, "\n", obj, "\n", obj.GetType().Name);
            }
            return default;
        }
        public static object DeserializeObject(byte[] bytes) {
            try {
                if (bytes == null)
                    return default;

                using MemoryStream memoryStream = new MemoryStream(Decompress(bytes));

                memoryStream.Seek(0, SeekOrigin.Begin);

                return binaryFormatter.Deserialize(memoryStream);
            }
            catch (System.Exception e) {
                ConOut.Error("DeserializeObject:", e, "\n", Utility.ArrayToString(bytes, 32));
            }
            return default;
        }

        public static byte[] Compress(byte[] data) {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionMode.Compress)) {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
        public static byte[] Decompress(byte[] data) {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress)) {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        sealed class PreMergeToMergedDeserializationBinder : SerializationBinder {

            public override System.Type BindToType(string assemblyName, string typeName) {
                string exeAssembly = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                return System.Type.GetType(string.Format("{0}, {1}", typeName, exeAssembly));
            }
        }
    }

    // [Who sends the packet]_[Name of the packet]

    interface IPacket {
        public virtual byte[] SerializeBinary() {
            return PacketHandler.SerializeObject(this);
        }
        public static IPacket DeserializeBinary(byte[] data) {
            return (IPacket)PacketHandler.DeserializeObject(data);
        }
    }

    [Serializable]
    struct Packet_Batch : IPacket {
        public byte[][] packets;
        [NonSerialized] public int size;

        /// <summary>
        /// In Bytes
        /// </summary>

        public Packet_Batch(IEnumerable<IPacket> packets) {
            this.packets = packets.Select(e => {
                byte[] data = e.SerializeBinary();
                return data;
            }).ToArray();
            size = this.packets.Length > 0 ? this.packets.Select(e => e.Length).Aggregate((a, b) => a + b) : 0;
        }

        public bool AddPacket(IPacket packet) {
            byte[] data = packet.SerializeBinary();
            if (size + data.Length > PacketHandler.PacketSizeLimit) return false;
            size += data.Length;
            packets = packets.Concat(new byte[][] { data }).ToArray();
            return true;
        }
    }
}
