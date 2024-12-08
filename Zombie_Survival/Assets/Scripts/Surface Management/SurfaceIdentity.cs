using UnityEngine;

namespace HQFPSWeapons
{
	public class SurfaceIdentity : MonoBehaviour
	{
		public SurfaceInfo Surface { get => m_Surface; }

		// TODO: REMOVE
        public Texture SurfaceTexture { get => m_SurfaceTexture; set => m_SurfaceTexture = value; }

		[SerializeField]
		private SurfaceInfo m_Surface = null;

		[SerializeField]
		private Texture m_SurfaceTexture = null;


		public Texture GetSurfaceTexture()
		{
			return m_SurfaceTexture;
		}
	}
}
