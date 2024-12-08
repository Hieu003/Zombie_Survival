using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HQFPSWeapons
{
	public struct SurfaceEffectSpawnEvent
	{
		public GameObject EffectObj;
		public int SurfaceId;
		public SurfaceEffects EffectType;
		public float AudioVolume;
	}

	public class TerrainInfo
	{
		public Vector3 Position { get; private set; }
		public TerrainData Data { get; private set; }

		private Terrain m_Terrain;
		private TerrainLayer[] m_Layers;


		public TerrainInfo(Terrain terrain)
		{
			Position = terrain.GetPosition();

			m_Terrain = terrain;
			Data = terrain.terrainData;
			m_Layers = Data.terrainLayers;
		}

		public Texture GetSplatmapPrototypeId(int i)
		{
			if(m_Layers != null && m_Layers.Length > i)
				return m_Layers[i].diffuseTexture;

			return null;
		}

		public float[,,] GetAlphamaps(int x, int y, int width, int height)
		{
			return m_Terrain.terrainData.GetAlphamaps(x, y, width, height);
		}
	}

	/// <summary>
	/// Global surface effects system
	/// </summary>
	public class SurfaceManager : Singleton<SurfaceManager>
	{
		[SerializeField]
		private bool m_SpatializeAudio = false;

		[SerializeField]
		private SurfaceInfo m_DefaultSurface;

		[Space]

		[SerializeField]
		private string m_SurfacesPath = "Surfaces/";

		private SurfaceInfo[] m_Surfaces;
		private Dictionary<Collider, TerrainInfo> m_SceneTerrains;


		public static void SpawnEffect(RaycastHit hitInfo, SurfaceEffects effectType, float audioVolume)
		{
			SpawnEffect(hitInfo, effectType, audioVolume, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
		}

		public static void SpawnEffect(RaycastHit hitInfo, SurfaceEffects effectType, float audioVolume, Vector3 position, Quaternion rotation)
		{
			SurfaceInfo surfInfo = Instance.Internal_GetSurfaceInfo(hitInfo);

			if(surfInfo != null)
			{
				PoolableObject effect = PoolingManager.Instance.GetObject(surfInfo.GetInstanceID().ToString() + effectType.ToString());

				if(effect != null)
				{
					effect.transform.position = position;
					effect.transform.rotation = rotation;
					effect.GetComponent<SurfaceEffect>().Play(audioVolume);
				}
			}
		}

		public static void SpawnEffect(int surfaceId, SurfaceEffects effectType, float audioVolume, Vector3 position, Quaternion rotation)
		{
			SurfaceInfo surfInfo = Instance.GetSurfaceWithId(surfaceId);

			PoolableObject effect = PoolingManager.Instance.GetObject(surfInfo.GetInstanceID().ToString() + effectType.ToString());

			if(effect != null)
			{
				effect.transform.position = position;
				effect.transform.rotation = rotation;
				effect.GetComponent<SurfaceEffect>().Play(audioVolume);
			}
		}

		public static SurfaceInfo GetSurfaceInfo(RaycastHit hitInfo)
		{
			return Instance.Internal_GetSurfaceInfo(hitInfo);
		}

		private SurfaceInfo Internal_GetSurfaceInfo(RaycastHit hitInfo)
		{
			if(m_Surfaces.Length == 0)
				return null;

			//Check if the collider has a surface identity component beforehand, if not get the texture from the renderer
			if (hitInfo.collider.TryGetComponent(out SurfaceIdentity surfId))
			{
				foreach (var surfInfo in m_Surfaces)
				{
					if (surfInfo.name == surfId.Surface.name)
						return surfInfo;
				}			
			}

			Texture texture;
			TerrainInfo terrainInfo;

			if(m_SceneTerrains.TryGetValue(hitInfo.collider, out terrainInfo))
			{
				float[] textureMix = GetTerrainTextureMix(hitInfo.point, terrainInfo.Data, terrainInfo.Position);
				int textureIndex = GetTerrainTextureIndex(textureMix);

				texture = terrainInfo.GetSplatmapPrototypeId(textureIndex);
			}
			else
				texture = GetMeshTextureId(hitInfo.collider, hitInfo.triangleIndex);

			if(texture != null)
			{
				for(int i = 0;i < m_Surfaces.Length;i++)
				{
					if(m_Surfaces[i].HasTexture(texture))
						return m_Surfaces[i];
				}
			}

			return m_DefaultSurface;
		}

		private SurfaceInfo GetSurfaceWithId(int surfaceId)
		{
			if(m_Surfaces.Length > surfaceId && surfaceId >= 0)
				return m_Surfaces[surfaceId];

			return null;
		}

		private void Awake()
		{
			m_Surfaces = Resources.LoadAll<SurfaceInfo>(m_SurfacesPath);

			for(int i = 0;i < m_Surfaces.Length;i++)
			{
				string surfName = m_Surfaces[i].name;

				m_Surfaces[i] = Instantiate(m_Surfaces[i]);
				m_Surfaces[i].name = surfName;

				m_Surfaces[i].CacheTextures();
			}

			SceneManager.sceneLoaded += CacheTerrains;
			CacheTerrains(new Scene(), LoadSceneMode.Single);

			CacheSurfaceEffects();
		}

		private void CacheSurfaceEffects()
		{
			foreach(var surfInfo in m_Surfaces)
			{
				string surfInstanceId = surfInfo.GetInstanceID().ToString();

				CreatePoolForEffect(surfInfo.name + "_" + SurfaceEffects.SoftFootstep.ToString(), surfInfo.SoftFootstepEffect, 25, 50, true, surfInstanceId + SurfaceEffects.SoftFootstep.ToString());
				CreatePoolForEffect(surfInfo.name + "_" + SurfaceEffects.HardFootstep.ToString(), surfInfo.HardFootstepEffect, 25, 50, true, surfInstanceId + SurfaceEffects.HardFootstep.ToString());
				CreatePoolForEffect(surfInfo.name + "_" + SurfaceEffects.FallImpact.ToString(), surfInfo.FallImpactEffect, 25, 50, true, surfInstanceId + SurfaceEffects.FallImpact.ToString());

				CreatePoolForEffect(surfInfo.name + "_" + SurfaceEffects.BulletHit.ToString(), surfInfo.BulletHitEffect, 50, 100, true, surfInstanceId + SurfaceEffects.BulletHit.ToString());
				CreatePoolForEffect(surfInfo.name + "_" + SurfaceEffects.Slash.ToString(), surfInfo.SlashEffect, 25, 50, true, surfInstanceId + SurfaceEffects.Slash.ToString());
				CreatePoolForEffect(surfInfo.name + "_" + SurfaceEffects.Stab.ToString(), surfInfo.StabEffect, 25, 50, true, surfInstanceId + SurfaceEffects.Stab.ToString());

				if(m_DefaultSurface != null && m_DefaultSurface.name == surfInfo.name)
					m_DefaultSurface = surfInfo;
			}
		}

		private void CreatePoolForEffect(string name, SurfaceInfo.EffectPair effectPair, int poolSizeMin, int poolSizeMax, bool autoShrink, string poolId)
		{
			GameObject effectTemplate = new GameObject(name);
			SurfaceEffect effectComponent = effectTemplate.AddComponent<SurfaceEffect>();
			effectComponent.Init(effectPair.AudioEffect, effectPair.VisualEffect, m_SpatializeAudio);

			PoolingManager.Instance.CreatePool(effectTemplate, poolSizeMin, poolSizeMin, autoShrink, poolId, 5f);

			Destroy(effectTemplate);
		}

		private void CacheTerrains(Scene scene, LoadSceneMode loadSceneMode)
		{
			m_SceneTerrains = new Dictionary<Collider, TerrainInfo>();
			Terrain[] sceneTerrains = FindObjectsOfType<Terrain>();

			for(int i = 0;i < sceneTerrains.Length;i++)
			{
				TerrainCollider terrainCollider = sceneTerrains[i].GetComponent<TerrainCollider>();

				if(terrainCollider == null)
					continue;

				m_SceneTerrains.Add(terrainCollider, new TerrainInfo(sceneTerrains[i]));
			}
		}

		private Texture GetMeshTextureId(Collider collider, int triangleIndex)
		{
			Renderer renderer = collider.GetComponent<Renderer>();
			MeshCollider meshCollider = collider as MeshCollider;

			if(!renderer || !renderer.sharedMaterial || !renderer.sharedMaterial.mainTexture)
				return null;
			else if(!meshCollider || meshCollider.convex)
				return renderer.material.mainTexture;

			Mesh mesh = meshCollider.sharedMesh;

			int materialIndex = -1;
			int lookupIndex1 = mesh.triangles[triangleIndex * 3];
			int lookupIndex2 = mesh.triangles[triangleIndex * 3 + 1];
			int lookupIndex3 = mesh.triangles[triangleIndex * 3 + 2];

			for(int i = 0;i < mesh.subMeshCount;i++)
			{
				int[] triangles = mesh.GetTriangles(i);

				for(int j = 0;j < triangles.Length;j += 3)
				{
					if(triangles[j] == lookupIndex1 && triangles[j + 1] == lookupIndex2 && triangles[j + 2] == lookupIndex3)
					{
						materialIndex = i;
						break;
					}
				}

				if(materialIndex != -1)
					break;
			}

			return renderer.materials[materialIndex].mainTexture;
		}

		private float[] GetTerrainTextureMix(Vector3 worldPos, TerrainData terrainData, Vector3 terrainPos)
		{
			// Returns an array containing the relative mix of textures
			// on the terrain at this world position.

			// The number of values in the array will equal the number
			// of textures added to the terrain.

			// Calculate which splat map cell the worldPos falls within (ignoring y)
			int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
			int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

			// Get the splat data for this cell as a 1x1xN 3D array (where N = number of textures)
			float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

			// Extract the 3D array data to a 1D array:
			float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

			for(int n = 0;n < cellMix.Length;n++)
				cellMix[n] = splatmapData[0, 0, n];

			return cellMix;
		}

		private int GetTerrainTextureIndex(float[] textureMix)
		{
			// Returns the zero-based index of the most dominant texture
			float maxMix = 0;
			int maxIndex = 0;

			// Loop through each mix value and find the maximum.
			for(int n = 0;n < textureMix.Length;n++)
			{
				if(textureMix[n] > maxMix)
				{
					maxIndex = n;
					maxMix = textureMix[n];
				}
			}

			return maxIndex;
		}
	}
}