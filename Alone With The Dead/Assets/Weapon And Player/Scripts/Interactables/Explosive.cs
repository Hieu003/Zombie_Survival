using System.Collections;
using System.Security;
using UnityEngine;
using UnityEngine.Events;

namespace HQFPSWeapons
{
	public class Explosive : Projectile
	{
		[SerializeField]
		public bool m_DetonateOnImpact = false;

		[ShowIf("m_DetonateOnImpact", true)]
		[SerializeField]
		[Range(0f, 15f)]
		private float m_DetonationDelay = 1.5f;

		[SerializeField]
		private GameObject m_ObjectToDisable = null;

		[SerializeField]
		private Explosion m_Explosion = null;

		[SerializeField]
		private LingeringFire m_LingeringFire = null;

		[Space]

		[SerializeField]
		private UnityEvent m_OnExplosiveLaunched = null;

		private LivingEntity m_Detonator;
		private bool m_IsDetonating;


		public override void Launch(LivingEntity launcher)
		{
			if(m_IsDetonating)
				return;

			m_IsDetonating = true;

			m_OnExplosiveLaunched.Invoke();

			if(!m_DetonateOnImpact)
				StartCoroutine(C_DetonateWithDelay(launcher));
			else
				m_Detonator = launcher;
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (m_DetonateOnImpact && m_IsDetonating)
				StartCoroutine(C_DetonateWithDelay(m_Detonator));
		}

		private IEnumerator C_DetonateWithDelay(LivingEntity launcher)
		{
			m_DetonateOnImpact = false;

			if (m_ObjectToDisable != null)
				m_ObjectToDisable.SetActive(false);

			yield return new WaitForSeconds(m_DetonationDelay);

			if (m_Explosion != null)
			{
				m_Explosion.transform.SetParent(null, true);
				m_Explosion.gameObject.SetActive(true);
				m_Explosion.Explode(launcher);
			}

			if (m_LingeringFire != null)
			{
				m_LingeringFire.transform.SetParent(null, true);
				m_LingeringFire.transform.rotation = Quaternion.identity;
				m_LingeringFire.gameObject.SetActive(true);
				m_LingeringFire.StartFire();
			}

			Destroy(gameObject);
		}
	}
}