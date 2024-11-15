using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	public class WaterBarrel : MonoBehaviour, IDamageable 
	{
		[SerializeField]
		private AudioSource m_AudioSource = null;

		[SerializeField]
		private Rigidbody m_Rigidbody = null;

		[SerializeField]
		private float m_MassWithoutWater = 15f;

		[SerializeField]
		private ParticleSystem m_WaterFX = null;

		[SerializeField]
		private float m_WaterQuantity = 10f;

		[SerializeField]
		private float m_DefaultStartSpeed = 2f;

		private List<ParticleSystem> m_WaterLeaks = new List<ParticleSystem>();

		private float m_CurrentWaterQuantity;
		private float m_InitialMass;


		public void TakeDamage(HealthEventData damage)
		{
			if(m_CurrentWaterQuantity > 0f && damage.HitPoint != Vector3.zero && damage.HitDirection != Vector3.zero)
			{
				ParticleSystem waterLeak = (ParticleSystem)Instantiate(m_WaterFX, damage.HitPoint, Quaternion.LookRotation(damage.HitNormal == Vector3.zero ? -damage.HitDirection : damage.HitNormal));
				waterLeak.transform.SetParent(transform);

				m_WaterLeaks.Add(waterLeak);

				if(m_WaterLeaks.Count == 1)
					m_AudioSource.Play();
			}
		}

		public LivingEntity GetEntity()
		{
			return null;
		}

		private void Start()
		{
			m_CurrentWaterQuantity = m_WaterQuantity;
			m_InitialMass = m_Rigidbody.mass;
		}

		private void Update()
		{
			if(m_CurrentWaterQuantity <= 0 || m_WaterLeaks.Count == 0)
				return;

			m_CurrentWaterQuantity -= Time.deltaTime * m_WaterLeaks.Count;

			for(int i = 0;i < m_WaterLeaks.Count;i ++)
			{
				var currentSpeed = Mathf.Clamp(m_DefaultStartSpeed * (m_CurrentWaterQuantity / m_WaterQuantity), 0f, m_DefaultStartSpeed);

				var main = m_WaterLeaks[i].main;
				main.startSpeedMultiplier = currentSpeed;

				if(currentSpeed <= 0f)
				{
					m_WaterLeaks[i].Stop();
					m_AudioSource.Stop();
				}
			}

			m_Rigidbody.mass = m_MassWithoutWater + (m_InitialMass - m_MassWithoutWater) * (m_CurrentWaterQuantity / m_WaterQuantity);
		}
	}
}