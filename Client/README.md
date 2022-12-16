# Global

## ChunkObject
Stores the voxels.
Has function: GenerateVoxels, GenerateMesh
Has a reference to a RenderObject.
## ChunkManager
Stroes the chunks
Has function: CreateChunk, GetChunkFromWorldPos, SetVoxel, GetVoxel
## VoxelObject
ushort with the block id.

## Vector3Byte
Three byte values, can convert from Vector3 and back.
## Vector3Sbyte
Three sbyte values, can convert from Vector3 and back.
## Vector3Int
Three int values, can convert from Vector3 and back.

//## PacketHandler
Handles the recieving and sending of packets between the server and client.

# Server


# Client
Everything related to rendering

## ClientChunkManager
Holds a ChunkManager, connects to its event handler, Handles chunk rendering and RenderObjects.
## ClientGameManager