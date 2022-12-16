using System.Collections.Generic;
using System.Numerics;

using VoxelEngine.Engine.Misc;
using VoxelEngine.Engine.World;
using VoxelEngine.Client.Rendering;
using System.Linq;
using VoxelEngine.Engine.Physics;

namespace VoxelEngine.Client {
	class ClientChunkManager {
		public static ClientChunkManager m_instance;

		private Dictionary<Vector3, RenderObject> chunkRenderObjects;
		private Queue<Vector3> ChunksRenderMesh;
		public ChunkManager chunkManager;

		private Shader chunk_shader;
		private Texture chunk_texture_diff;
		private Texture chunk_texture_spec;

		public ClientChunkManager() {
			if (m_instance != null) ConOut.Warn("There's more than one client chunk manager!");
			m_instance = this;

			chunkRenderObjects = new Dictionary<Vector3, RenderObject>();
			ChunksRenderMesh = new Queue<Vector3>();

			chunk_shader = Shader.CreateFromFiles(FileManager.ResourcesPath + "Shaders/default.frag", FileManager.ResourcesPath + "Shaders/default.vert");
			chunk_texture_diff = new Texture(FileManager.ResourcesPath + "Textures/voxel_diff.png");
			chunk_texture_spec = new Texture(FileManager.ResourcesPath + "Textures/voxel_spec.png");

			chunkManager = new ChunkManager();

			chunkManager.ChunkCreated += (chunk, chunk_pos) => {
				chunkRenderObjects.Add(chunk_pos, new RenderObject(new Mesh(), chunk_shader, chunk_texture_diff, chunk_texture_spec));
				ChunksRenderMesh.Enqueue(chunk_pos);
			};
			chunkManager.ChunkRecieved += (chunk, chunk_pos) => {
				chunkRenderObjects.Add(chunk_pos, new RenderObject(new Mesh(), chunk_shader, chunk_texture_diff, chunk_texture_spec));
				UpdateChunkAndSurroundingChunks((ChunkObject)chunk);
			};
			chunkManager.ChunkVoxelUpdate += (chunk, world_pos) => {
				VoxelUpdateChunkAndSurroundingChunk(world_pos, chunk as ChunkObject);
			};
			chunkManager.ChunkBulkVoxelUpdate += (_, world_poss) => {
				/*// We don't want to update a chunk more than once so we filter them and convert them
				var world_pos_filtered = world_poss.Select(e => chunkManager.GetChunkPosition(e));
				world_pos_filtered = world_pos_filtered.Where((e, i) => System.Array.IndexOf(world_poss, e) == i);*/
				foreach (var item in world_poss)
					AddChunkToRenderMesh(chunkManager.GetChunkFromWorldPos(item));
			};
		}

		public void Render(bool shadowMap) {
			for (int i = 0; i < chunkManager.ActiveChunks.Count; i++) {
				Vector3 chunk_pos = chunkManager.ActiveChunks[i];
				Matrix4x4 model = Matrix4x4.CreateTranslation(chunkManager.GetChunk(chunk_pos).CHUNK_WORLD_POS);
				chunkRenderObjects[chunk_pos].Render(model, shadowMap);
			}
		}
		public void UpdateChunkAndSurroundingChunks(ChunkObject chunk) {
			if (chunk == null) return;
			AddChunkToRenderMesh(chunk);
			for (int i = 0; i < 6; i++) {
				Vector3 other_chunk_pos = PhysicsUtility.faceChecks[i] + chunk.CHUNK_POS;
				AddChunkToRenderMesh(chunkManager.GetChunk(other_chunk_pos));
			}
		}
		public void VoxelUpdateChunkAndSurroundingChunk(Vector3 world_pos, ChunkObject chunk) {
			AddChunkToRenderMesh(chunk);
			for (int i = 0; i < 6; i++) {
				Vector3 local_pos_face = PhysicsUtility.faceChecks[i] + world_pos;
				Vector3 other_chunk_pos = ChunkManager.GetChunkPosition(local_pos_face);
				if (other_chunk_pos != chunk.CHUNK_POS)
					AddChunkToRenderMesh(chunkManager.GetChunk(other_chunk_pos));
			}
		}
		public void AddChunkToRenderMesh(ChunkObject chunk) {
			if (chunk != null && !ChunksRenderMesh.Contains(chunk.CHUNK_POS))
				ChunksRenderMesh.Enqueue(chunk.CHUNK_POS);
		}

		public void Update() {
			if (ChunksRenderMesh.Count > 0) {
				Vector3 chunk_pos = ChunksRenderMesh.Dequeue();
				chunkRenderObjects[chunk_pos].MESH.SetVerticies(VoxelMeshGenerator.GenerateChunkVoxelMesh(chunkManager.GetChunk(chunk_pos)));
			}
		}
	}
}
