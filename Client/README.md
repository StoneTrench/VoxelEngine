world_pos - it's the global position of a voxel
chunk_pos - the position of a chunk on the chunk grid
chunk_world_pos - position of the chunk in the world
local_pos - position of a voxel in a chunk
global_pos - just a float vector3 coordinate
region_pos - position of a region (8 x 8 x 8 chunks in size)

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