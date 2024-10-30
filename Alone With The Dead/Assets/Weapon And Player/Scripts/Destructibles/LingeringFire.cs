using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	public class LingeringFire : MonoBehaviour
	{
		[SerializeField]
		private LightEffect m_LightEffect = null;

		[SerializeField]
		[Clamp(0f, 1000f)]
		private float m_DamagePerSecond = 20f;

		[SerializeField]
		[Range(0f, 10f)]
		private float m_LingeringTime = 5f;

		[SerializeField]
		private float m_StopDuration = 1f;

		[SerializeField]
		private LayerMask m_LayerMask = new LayerMask();

		private ParticleSystem m_FireParticles;
		private AudioSource m_AudioS;
		private bool m_CanDamage = true;
		private List<LivingEntity> m_AffectedEntities = new List<LivingEntity>();
		private float m_DamageMultiplier = 1f;


		public void StartFire() 
		{
			m_AudioS = GetComponent<AudioSource>();
			m_FireParticles = GetComponent<ParticleSystem>();

			m_FireParticles.Stop();
			var main = m_FireParticles.main;

			main.duration = m_LingeringTime;
			m_FireParticles.Play();

			m_LightEffect.Play(true);

			PositionFire();
			StartCoroutine(C_FireLingering());
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.GetComponent<LivingEntity>() != null)
				m_AffectedEntities.Add(other.GetComponent<LivingEntity>());
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.GetComponent<LivingEntity>() != null) 
				m_AffectedEntities.Remove(other.GetComponent<LivingEntity>());
		}

		private void OnTriggerStay(Collider other)
		{
			Fire();
		}

		private void PositionFire() 
		{
			RaycastHit hit;

			if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, m_LayerMask))
			{
				transform.position = hit.point;
			}
		}

		private void Fire() 
		{
			if (!m_CanDamage)
				return;

			foreach (var entity in m_AffectedEntities)
			{
				if (entity != null)
					entity.ChangeHealth.Try(new HealthEventData(-m_DamagePerSecond * m_DamageMultiplier * Time.deltaTime));
			}	
		}

		IEnumerator C_FireLingering() 
		{
			yield return new WaitForSeconds(m_LingeringTime);

			m_FireParticles.Stop();
			m_LightEffect.Stop(true);

			float stopDuration = Time.time + m_StopDuration;

			WaitForEndOfFrame wait = new WaitForEndOfFrame();

			while (stopDuration > Time.time)
			{
				m_AudioS.volume -= Time.deltaTime * ( 1 / m_StopDuration);
				m_DamageMultiplier -= Time.deltaTime * (1 / m_StopDuration);

				yield return wait;
			}

			m_CanDamage = false;
		}
	}
}