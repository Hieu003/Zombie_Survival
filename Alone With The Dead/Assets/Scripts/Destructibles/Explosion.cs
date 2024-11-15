using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	public class Explosion : MonoBehaviour
	{
		[SerializeField]
		private bool m_DetonateOnStart = false;

		[SerializeField]
		private float m_Force = 105f;

		[SerializeField]
		[Clamp(0f, 1000f)]
		private float m_Damage = 100f;

		[SerializeField]
		private float m_Radius = 15f;

		[SerializeField]
		[Range(0f, 10f)]
		private float m_Scale = 1f;


		public void Explode(LivingEntity detonator)
		{
			ShakeManager.ShakeEvent.Send(new ShakeEventData(transform.position, m_Radius, m_Scale, ShakeType.Explosion));

			var cols = Physics.OverlapSphere(transform.position, m_Radius);
			var rigidbodies = new List<Rigidbody>();

			foreach(var col in cols)
			{
				if(col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
					rigidbodies.Add(col.attachedRigidbody);

				var entity = col.GetComponent<LivingEntity>();

				if(entity != null)
				{
					float distToObject = (transform.position - col.transform.position).sqrMagnitude;
					float explosionRadiusSqr = m_Radius * m_Radius;

					float distanceFactor = 1f - Mathf.Clamp01(distToObject / explosionRadiusSqr);

					entity.ChangeHealth.Try(new HealthEventData(
						-m_Damage * distanceFactor, 
						transform.position, 
						(col.transform.position - transform.position).normalized,
						m_Force,
						Vector3.zero,
						null
					));
				}

				foreach(var rb in rigidbodies)
				{
					var damageable = rb.GetComponent<IDamageable>();
					rb.AddExplosionForce((damageable == null || damageable.GetEntity() == null) ? m_Force : m_Force / Mathf.Max(1, damageable.GetEntity().Hitboxes.Length), transform.position, 2f, m_Radius, ForceMode.Impulse);
				}
			}
		}

		private IEnumerator Start()
		{
			yield return null;

			if(m_DetonateOnStart)
				Explode(null);
		}
	}
}