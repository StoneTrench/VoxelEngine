using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using VoxelEngine.Engine.Entities;
using VoxelEngine.Engine.World;

namespace VoxelEngine.Engine.Networking {

    #region Client
	[Serializable]
    struct Client_ConnetionToServerRequest : IPacket {
        public string username;

        public Client_ConnetionToServerRequest(string username) {
            this.username = username;
        }
    }

    [Serializable]
    struct Client_ChatMessage : IPacket {
        public string text;

        public Client_ChatMessage(string text) {
            this.text = text;
        }
    }

    [Serializable]
    struct Client_PlayerPosition : IPacket {
        public float[] position;
        public byte renderDistance;

        public Client_PlayerPosition(Vector3 position, byte renderDistance) {
            this.position = new SerializableVector3(position).array;
            this.renderDistance = (byte)renderDistance;
        }
    }

    [Serializable]
    struct Client_BlockBreak : IPacket {
        public int position_x;
        public int position_y;
        public int position_z;

        public Client_BlockBreak(Vector3 world_pos) {
            this.position_x = (int)world_pos.X;
            this.position_y = (int)world_pos.Y;
            this.position_z = (int)world_pos.Z;
        }

        public Vector3 position {
            get {
                return new Vector3(position_x, position_y, position_z);
            }
        }
    }

    [Serializable]
    struct Client_BlockPlace : IPacket {
        public int position_x;
        public int position_y;
        public int position_z;
        public ushort voxel_id;

        public Client_BlockPlace(Vector3 world_pos, ushort voxel_id) {
            this.position_x = (int)world_pos.X;
            this.position_y = (int)world_pos.Y;
            this.position_z = (int)world_pos.Z;
            this.voxel_id = voxel_id;
        }

        public Vector3 position {
            get {
                return new Vector3(position_x, position_y, position_z);
            }
        }
    }
	#endregion

	#region Server
	[Serializable]
    struct Server_ConnetionToServerResponse : IPacket {
        public string[] onlinePlayers;

        public Server_ConnetionToServerResponse(string[] onlinePlayers) {
            this.onlinePlayers = onlinePlayers;
        }
    }
    
    [Serializable]
    struct Server_ChunkData : IPacket {
        public int[] chunk_pos;

        public SerializableVoxelObject[] palette;
        public short[] voxels;

        public Server_ChunkData(ChunkObject chunk) {
            List<SerializableVoxelObject> palette = new List<SerializableVoxelObject>();
            List<short> voxels = new List<short>();

            for (int x = 0; x < ChunkManager.CHUNK_SIZE.x; x++) {
                for (int y = 0; y < ChunkManager.CHUNK_SIZE.y; y++) {
                    for (int z = 0; z < ChunkManager.CHUNK_SIZE.z; z++) {
                        var vox = new SerializableVoxelObject(chunk.voxels[x, y, z]);
                        int index = palette.IndexOf(vox);
                        if (index == -1) {
                            voxels.Add((short)palette.Count);
                            palette.Add(vox);
                        }
                        else
                            voxels.Add((short)index);
                    }
                }
            }

            this.palette = palette.ToArray();
            this.voxels = voxels.ToArray();

            chunk_pos = new SerializableVector3Int(chunk.CHUNK_POS).array;
        }
    }

    [Serializable]
    struct Server_ChatMessage : IPacket {
        public string text;

        public Server_ChatMessage(string text) {
            this.text = text;
        }
    }

    [Serializable]
    struct Server_BlockUpdate : IPacket {
        public int position_x;
        public int position_y;
        public int position_z;
        public SerializableVoxelObject newVoxel;

        public Server_BlockUpdate(Vector3 world_pos, VoxelObject newVoxel) {
            this.position_x = (int)world_pos.X;
            this.position_y = (int)world_pos.Y;
            this.position_z = (int)world_pos.Z;
            this.newVoxel = new SerializableVoxelObject(newVoxel);
        }

        public Vector3 position {
            get {
                return new Vector3(position_x, position_y, position_z);
            }
        }
    }

    [Serializable]
    struct Server_BulkBlockUpdates : IPacket {
        public int[] positions_x;
        public int[] positions_y;
        public int[] positions_z;
        public ushort[] newVoxels;

        public Server_BulkBlockUpdates(Vector3[] world_poss, VoxelObject[] newVoxels) {
            this.positions_x = new int[world_poss.Length];
            this.positions_y = new int[world_poss.Length];
            this.positions_z = new int[world_poss.Length];
            this.newVoxels = new ushort[newVoxels.Length];

			for (int i = 0; i < world_poss.Length; i++) {
                this.positions_x[i] = (int)world_poss[i].X;
                this.positions_y[i] = (int)world_poss[i].Y;
                this.positions_z[i] = (int)world_poss[i].Z;
                this.newVoxels[i] = newVoxels[i].VOXEL_ID;
            }
        }
    }
    
    [Serializable]
    struct Server_EntityData : IPacket {
        public EntityData[] entities;

        public Server_EntityData(IEnumerable<Entity> entity) {
            entities = entity.Select(e => e.entityData).ToArray();
        }
    }
	#endregion

}
