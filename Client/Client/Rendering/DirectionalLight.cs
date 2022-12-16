using System.Numerics;
using VoxelEngine.Engine.Misc;

namespace VoxelEngine.Client.Rendering {
	class DirectionalLight {
		public static DirectionalLight m_instance;

		public Vector3 DIRECTION;

		public DirectionalLight(Vector3 dIRECTION) {
			if (m_instance != null) ConOut.Warn("There's more than one directional light!");
			m_instance = this;

			DIRECTION = Vector3.Normalize(dIRECTION);
		}
	}
}
