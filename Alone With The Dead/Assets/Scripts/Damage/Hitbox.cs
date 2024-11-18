using UnityEngine;
using UnityEngine.Events;
using System;

namespace HQFPSWeapons
{
	/// <summary>
	/// Will register damage events from outside and pass them to the parent entity.
	/// </summary>
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	public class Hitbox : MonoBehaviour, IDamageable 
	{
		#region Internal
		[Serializable]
		public class DamageEvent : UnityEvent<HealthEventData>
		{ }

		[Serializable]
		public class DamageEventSimple : UnityEvent<float>
		{ }
        #endregion

        public Collider Collider { get { return m_Collider; } }

		[SerializeField]
		[Clamp(0f, Mathf.Infinity)]
		private float m_DamageMultiplier = 1f;

		[SerializeField]
		private SoundPlayer m_GroundImpactSound = null;

		[SerializeField]
		private DamageEvent m_OnDamageEvent = null;

		[SerializeField]
		private DamageEventSimple m_OnDamageEventSimple = null;

		private Collider m_Collider;
		private Rigidbody m_Rigidbody;
		private LivingEntity m_ParentEntity;

		private bool m_HitboxImpact;

        private ZombieHealth zombieHealth;

        private void Start()
        {
            zombieHealth = GetComponentInParent<ZombieHealth>();
        }

        public void TakeDamage(HealthEventData damageData)
		{
			if(enabled)
			{
				m_OnDamageEvent.Invoke(damageData);
				m_OnDamageEventSimple.Invoke(damageData.Delta);

				if (m_ParentEntity != null)
				{
					if (m_ParentEntity.Health.Get() > 0f)
					{
						damageData.Delta *= m_DamageMultiplier;
						m_ParentEntity.ChangeHealth.Try(damageData);
					}

					if (m_ParentEntity.Health.Get() == 0f)
						m_Rigidbody.AddForceAtPosition(damageData.HitDirection * damageData.HitImpulse, damageData.HitPoint, ForceMode.Impulse);
				}
                if (zombieHealth != null)
                {
                    zombieHealth.TakeDamage(damageData);
                }
            }
        }
		

		public LivingEntity GetEntity()
		{
			return m_ParentEntity;
            return zombieHealth != null ? zombieHealth.GetEntity() : null;
        }

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.relativeVelocity.sqrMagnitude > 5f && !m_Rigidbody.isKinematic && !m_HitboxImpact)
			{
				m_GroundImpactSound.PlayAtPosition(ItemSelection.Method.RandomExcludeLast, transform.position, 1f);
				m_HitboxImpact = true;
			}
		}

		private void Awake()
		{
			m_ParentEntity = GetComponentInParent<LivingEntity>();

			m_Collider = GetComponent<Collider>();
			m_Rigidbody = GetComponent<Rigidbody>();

			m_ParentEntity.Respawn.AddListener(Respawn);
		}

		private void Respawn() 
		{
			m_HitboxImpact = false;
		}
	}
}
