using System;
using System.Collections;
using UnityEngine;

namespace HQFPSWeapons
{
	public class MeleeWeapon : EquipmentItem
	{
		[BHeader("MELEE WEAPON", true)]

		[SerializeField]
		[Group]
		private MeleeWeaponSettings m_MeleeWeaponSettings = null;

		[Space(3f)]

		[BHeader("SWINGS", order = 100)]

		[SerializeField]
		private ItemSelection.Method m_SwingSelection = ItemSelection.Method.RandomExcludeLast;

		[SerializeField]
		[Group]
		private SwingData[] m_Swings = null;

		private int m_LastFreeSwing;
		private float m_NextResetSwingSelectionTime;


		public override bool TryUseOnce(Camera camera)
		{
			if(Time.time < m_NextTimeCanUse)
				return false;

			SwingData swing = null;

			//Select Swing
			if (swing == null)
			{
				if (Time.time > m_NextResetSwingSelectionTime && m_MeleeWeaponSettings.ResetSwingsIfNotUsed)
					swing = m_Swings[0];
				else
					swing = m_Swings.Select(ref m_LastFreeSwing, m_SwingSelection);
			}

			m_UseThreshold = swing.Cooldown;
			m_NextTimeCanUse = Time.time + m_UseThreshold;

			if (m_MeleeWeaponSettings.ResetSwingsIfNotUsed)
				m_NextResetSwingSelectionTime = Time.time + m_MeleeWeaponSettings.ResetSwingsDelay;

			m_EHandler.Animator.SetFloat("Swing Speed", swing.AnimationSpeed);
			m_EHandler.Animator.SetFloat("Swing Index", swing.AnimationIndex);
			m_EHandler.Animator.SetTrigger("Swing");

			m_EHandler.PlayCameraForce(swing.SwingCamForce);

			Player.Camera.AddPositionForce(swing.SwingCamPositionForce, swing.SwingCamForce.Force.Distribution);

			m_EHandler.PlayDelayedSound(swing.Audio);

			StartCoroutine(C_SphereCastDelayed(camera, swing));

			return true;
		}

		public override bool TryUseContinuously(Camera camera)
		{
			if (!m_MeleeWeaponSettings.CanContinuouslyAttack)
				return false;

			return TryUseOnce(camera);
		}

		protected virtual IDamageable SphereCast(Camera camera, SwingData swing)
		{
			IDamageable damageable = null;
			RaycastHit hitInfo;

			if(Physics.SphereCast(camera.transform.position, swing.CastRadius, camera.transform.forward, out hitInfo, m_MeleeWeaponSettings.CastDistance, m_MeleeWeaponSettings.HitMask, QueryTriggerInteraction.Ignore))
			{
				SurfaceManager.SpawnEffect(hitInfo, m_MeleeWeaponSettings.ImpactEffect, 1f);

				// Apply an impact impulse
				if(hitInfo.rigidbody != null)
					hitInfo.rigidbody.AddForceAtPosition(camera.transform.forward * swing.ImpactForcePerHit, hitInfo.point, ForceMode.Impulse);

				var damageData = new HealthEventData(-swing.DamagePerHit, m_MeleeWeaponSettings.DamageType, hitInfo.point, camera.transform.forward, swing.ImpactForcePerHit, Player);

				// Do damage
				damageable = hitInfo.collider.GetComponent<IDamageable>();

				if(damageable != null)
					damageable.TakeDamage(damageData);

				// Camera force
				m_EHandler.PlayCameraForce(swing.ImpactCamForce);
			}

			return damageable;
		}

		private IEnumerator C_SphereCastDelayed(Camera camera, SwingData swing)
		{
			yield return new WaitForSeconds(swing.CastDelay);

			SphereCast(camera, swing);
		}

		private void Start()
		{
			m_UseClickBuffer = true;
		}

		#region Internal
		[Serializable]
		private class MeleeWeaponSettings
		{
			public LayerMask HitMask = new LayerMask();

			[Range(0f, 3f)]
			[Tooltip("How far can this weapon hit stuff?")]
			public float CastDistance = 1.5f;

			public SurfaceEffects ImpactEffect = SurfaceEffects.Slash;

			public DamageType DamageType = DamageType.Hit;

			[Space]

			public bool CanContinuouslyAttack = false;

			public bool ResetSwingsIfNotUsed = false;

			[ShowIf("ResetSwingsIfNotUsed", true, 10f)]
			public float ResetSwingsDelay = 1f;
		}

        [Serializable]
		public class SwingData : CloneableObject<SwingData>
		{
			[BHeader("General", true)]

			public string Name = "Strong Attack";

			[Tooltip("Useful for limiting the number of hits you can do in a period of time.")]
			public float Cooldown = 1f;

			[Space(3)]

			[BHeader("Sphere Cast", order = 100)]

			public float CastDelay = 0.4f;

			public float CastRadius = 0.2f;

			[Space(3)]

			[BHeader("Impact", order = 100)]

			public float DamagePerHit = 15f;

			public float ImpactForcePerHit = 30f;

			[Space(3)]

			[BHeader("Audio", order = 100)]

			public DelayedSound Audio;

			[Space(3)]

			[BHeader("Animation", order = 100)]

			public int AnimationIndex;

			public float AnimationSpeed = 1f;

			[Space(3)]

			[BHeader("Camera Force", order = 100)]

			public DelayedCameraForce SwingCamForce;

			public Vector3 SwingCamPositionForce;

			public DelayedCameraForce ImpactCamForce;
		}
        #endregion
    }
}