using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	public class ItemSpawner : MonoBehaviour
	{
		[BHeader("General", true)]

		[SerializeField]
		private bool OneTimeSpawn = false;

		[SerializeField]
		private ItemSpawnSuccesion m_SpawnType = ItemSpawnSuccesion.InOrder;

		[SerializeField]
		private ItemToSpawn[] m_ItemsToSpawn = null;

		[SerializeField]
		private Vector3 m_RandomRotation = Vector3.zero;

		[SerializeField]
		private ParticleSystem m_ParticleEffects = null;

		[BHeader("Delays")]

		[SerializeField]
		private float m_CanSpawnDelay = 10f;

		[SerializeField]
		private float m_InitialItemSpawnDelay = 0.5f;

		[SerializeField]
		private float m_DelayBetweenItemSpawns = 0.1f;

		[SerializeField]
		private float m_ItemDestroyDelay = 15f;

		[BHeader("Audio")]

		[SerializeField]
		private SoundPlayer m_StartSpawnAudio = null;

		[SerializeField]
		private SoundPlayer m_EndSpawnAudio = null;

		private BoxCollider m_Collider;
		private AudioSource m_AudioSource;

		private int m_ItemsToSpawnCount;
		private float m_NextTimeCanSpawn;
		private bool m_CanSpawn = true;

		private WaitForSeconds m_TimeBetweenSpawns;


		public void SpawnItems(int maxSpawnCount)
		{
			if (Time.time > m_NextTimeCanSpawn && m_CanSpawn)
			{
				maxSpawnCount = Mathf.Clamp(maxSpawnCount, 0, m_ItemsToSpawn.Length);

				m_NextTimeCanSpawn = Time.time + m_CanSpawnDelay;

				List <ItemPickup> itemsToSpawn = new List<ItemPickup>();

				if (m_SpawnType == ItemSpawnSuccesion.InOrder)
				{
					for (int i = 0; i < m_ItemsToSpawn.Length; i++)
					{
						if (m_ItemsToSpawnCount >= maxSpawnCount)
							break;

						for (int n = 0; n < m_ItemsToSpawn[i].Amount; n++)
						{
							if (m_ItemsToSpawnCount >= maxSpawnCount)
								break;

							itemsToSpawn.Add(m_ItemsToSpawn[i].ItemPickup);

							m_ItemsToSpawnCount++;
						}
					}
				}
				else if (m_SpawnType == ItemSpawnSuccesion.Random)
				{
					for (int i = 0; i < maxSpawnCount; i++)
					{
						int randomIndex = Random.Range(i, m_ItemsToSpawn.Length);

						ItemToSpawn randomItem = m_ItemsToSpawn[randomIndex];

						if (m_ItemsToSpawn[i].ItemPickup != randomItem.ItemPickup)
						{
							ItemToSpawn temp = m_ItemsToSpawn[i];
							m_ItemsToSpawn[i] = randomItem;
							m_ItemsToSpawn[randomIndex] = temp;
						}

						itemsToSpawn.Add(randomItem.ItemPickup);
					}
				}

				StartCoroutine(C_SpawnItems(itemsToSpawn));
			}
		}

		private Vector3 RandomPointInBounds(Bounds bounds)
		{
			return new Vector3(
				Random.Range(bounds.min.x, bounds.max.x),
				Random.Range(bounds.min.y, bounds.max.y),
				Random.Range(bounds.min.z, bounds.max.z)
			);
		}

		private void Start()
		{
			m_Collider = GetComponent<BoxCollider>();
			m_AudioSource = GetComponent<AudioSource>();
			m_TimeBetweenSpawns = new WaitForSeconds(m_DelayBetweenItemSpawns);
		}

		private IEnumerator C_SpawnItems(List<ItemPickup> itemsToSpawn) 
		{
			yield return new WaitForSeconds(m_InitialItemSpawnDelay);

			m_StartSpawnAudio.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource);

			for (int i = 0; i < itemsToSpawn.Count; i++)
			{
				if (itemsToSpawn[i] != null)
				{
					Quaternion spawnedItemRotation = Quaternion.Euler(
						Random.Range(-Mathf.Abs(m_RandomRotation.x), Mathf.Abs(m_RandomRotation.x)),
						Random.Range(-Mathf.Abs(m_RandomRotation.y), Mathf.Abs(m_RandomRotation.y)),
						Random.Range(-Mathf.Abs(m_RandomRotation.z), Mathf.Abs(m_RandomRotation.z))
					);

					ItemPickup item = Instantiate(itemsToSpawn[i], RandomPointInBounds(m_Collider.bounds), spawnedItemRotation);
					item.StartCoroutine(item.C_DelayedDestroy(m_ItemDestroyDelay));

					if (m_ParticleEffects != null)
						Instantiate(m_ParticleEffects, item.transform.position, spawnedItemRotation);

					yield return m_TimeBetweenSpawns;
				}
			}

			if (OneTimeSpawn)
				m_CanSpawn = false;

			m_ItemsToSpawnCount = 0;

			m_EndSpawnAudio.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource);
		}

		// ------------------- Internal ------------------
		[Serializable]
		private struct ItemToSpawn
		{
			#pragma warning disable 0649

			public ItemPickup ItemPickup;
			public int Amount;

			#pragma warning restore 0649
		}

		public enum ItemSpawnSuccesion
		{
			InOrder,
			Random
		}
	}
}