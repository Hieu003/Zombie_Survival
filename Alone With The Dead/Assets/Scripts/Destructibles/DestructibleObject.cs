using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	public class DestructibleObject : MonoBehaviour, IDamageable
	{
		[Header("Health")]

		[SerializeField]
		[Clamp(0f, 100f)]
		private float m_InitialHealth = 100f;

		[SerializeField]
		private DamageResistance m_DamageResistance = null;

		[Header("Loot")]

		[SerializeField]
		private ItemPickupRandomList m_AllPossibleLoot = null;

		[SerializeField]
		[Range(0, 10)]
		[Tooltip("How many items will be spawned.")]
		private int m_LootGiveAmount = 1;

		[SerializeField]
		private Vector3 m_LootSpawnOffset = Vector3.zero;

		[Header("Debris")]

		[SerializeField]
		[Tooltip("Must be a child of the object.")]
		private GameObject m_DestroyedVersion = null;

		[Space]

		[SerializeField]
		private List<DebrisFragment> m_DebrisFragments = null;

		[SerializeField]
		private bool m_ApplyDefaultDebrisForce = false;

		[SerializeField]
		private float m_CustomDebrisForceMult = 1f;

		private float m_CurrentHealth;


		#if UNITY_EDITOR
		public void SetDebrisFragments(List<DebrisFragment> debrisFragments)
		{
			m_DebrisFragments = debrisFragments;
		}
		#endif

		public void TakeDamage(HealthEventData damageData)
		{
			float damage = -Mathf.Abs(damageData.Delta);
			damage *= (1f - m_DamageResistance.GetDamageResistance(damageData));

			m_CurrentHealth = Mathf.Clamp(m_CurrentHealth + damage, 0f, m_InitialHealth);

			if(m_CurrentHealth == 0f)
				DestroyObject(damageData);
		}

		public LivingEntity GetEntity()
		{
			return null;
		}
			
		private void Start()
		{
			m_CurrentHealth = m_InitialHealth;
		}

		private void DestroyObject(HealthEventData data)
		{
			SpawnLoot();

			m_DestroyedVersion.transform.SetParent(transform.parent);
			m_DestroyedVersion.SetActive(true);

			float customForcePerPiece = (data.HitImpulse * m_CustomDebrisForceMult) / m_DebrisFragments.Count;

			for(int i = 0;i < m_DebrisFragments.Count;i ++)
			{
				Vector3 customForceVecPerPiece = (data.HitDirection + Vector3.down + (m_DebrisFragments[i].Fragment.position - transform.position)) * customForcePerPiece;

				if(m_ApplyDefaultDebrisForce)
					m_DebrisFragments[i].ApplyDefaultForce();
				else
					m_DebrisFragments[i].ApplyCustomForce(customForceVecPerPiece, ForceMode.Impulse);
			}

			Destroy(gameObject);
		}

		private void SpawnLoot()
		{
			ItemPickup[] randomLoot;

			if(m_AllPossibleLoot.GetRandomItems(m_LootGiveAmount, out randomLoot))
			{
				for(int i = 0;i < randomLoot.Length;i ++)
					Instantiate(randomLoot[i], transform.position + transform.TransformVector(m_LootSpawnOffset), Quaternion.identity);
			}
		}


		// --------------- Internal ---------------
		[Serializable]
		public class DebrisFragment
		{
			public Rigidbody Fragment { get { return m_Piece; } }

			[SerializeField]
			private Rigidbody m_Piece;

			[Header("Default Force")]

			[SerializeField]
			private Vector3 m_DefaultForceMin;

			[SerializeField]
			private Vector3 m_DefaultForceMax;


			public DebrisFragment(Rigidbody rigidbody, Vector3 defaultForceMin = default(Vector3), Vector3 defaultForceMax = default(Vector3))
			{
				m_Piece = rigidbody;
				m_DefaultForceMin = defaultForceMin;
				m_DefaultForceMax = defaultForceMax;
			}

			public void ApplyDefaultForce()
			{
				Vector3 force = new Vector3(
					Random.Range(m_DefaultForceMin.x, m_DefaultForceMax.x), 
					Random.Range(m_DefaultForceMin.y, m_DefaultForceMax.y), 
					Random.Range(m_DefaultForceMin.z, m_DefaultForceMax.z));
				
				m_Piece.AddForce(force, ForceMode.Impulse);
			}

			public void ApplyCustomForce(Vector3 force, ForceMode forceMode)
			{
				m_Piece.AddForce(force, forceMode);
			}
		}
	}
}