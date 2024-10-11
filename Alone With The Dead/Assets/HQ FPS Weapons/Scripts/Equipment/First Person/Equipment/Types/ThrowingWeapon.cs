using System;
using System.Collections;
using UnityEngine;

namespace HQFPSWeapons
{
	public class ThrowingWeapon : EquipmentItem
	{
		[BHeader("THROWING WEAPON", true)]

		[SerializeField]
		[Group]
		private ThrowingWeaponGeneralModule m_ThrowingWeaponSettings = null;

		[Space]

		[SerializeField]
		[Group]
		private ThrowingSettings m_LongThrowSettings = null;

		[SerializeField]
		[Group]
		private ThrowingSettings m_ShortThrowSettings = null;
		 
		private bool m_IsThrown;


		public override void Wield(SaveableItem item)
		{
			base.Wield(item);

			m_IsThrown = false;
		}

		public override bool TryUseOnce(Camera camera)
		{
			if (m_IsThrown)
				return false;

			m_EHandler.Animator.SetFloat("Long Throw Speed", m_LongThrowSettings.AnimThrowSpeed);
			m_EHandler.Animator.SetTrigger("Long Throw");
			m_EHandler.PlaySounds(m_LongThrowSettings.ThrowAudio);

			StartCoroutine(C_ThrowWithDelay(camera, m_LongThrowSettings.SpawnDelay, 0));

			return true;
		}

		public override void OnAimStart()
		{
			if (m_IsThrown)
				return;

			m_EHandler.Animator.SetFloat("Short Throw Speed", m_LongThrowSettings.AnimThrowSpeed);
			m_EHandler.Animator.SetTrigger("Short Throw");
			m_EHandler.PlaySounds(m_ShortThrowSettings.ThrowAudio);

			StartCoroutine(C_ThrowWithDelay(m_EHandler.EquipmentManager.WorldCamera, m_ShortThrowSettings.SpawnDelay, 1));
		}

		private void Start()
		{
			EnableAiming = true;
		}

		private void Awake()
		{
			if(m_ThrowingWeaponSettings.ObjectToSpawn != null)
				PoolingManager.Instance.CreatePool(m_ThrowingWeaponSettings.ObjectToSpawn, 3, 6, true, m_ThrowingWeaponSettings.ObjectToSpawn.GetInstanceID().ToString(), 10);
		}

		private IEnumerator C_ThrowWithDelay(Camera camera, float delay, int throwIndex)
		{
			m_IsThrown = true;

			ThrowingSettings throwSettings = (throwIndex == 0) ? m_LongThrowSettings : m_ShortThrowSettings;
			float animSpeedFactor = 1f / throwSettings.AnimThrowSpeed;

			m_EHandler.PlayCameraForces(throwSettings.CameraForces);

			yield return new WaitForSeconds(throwSettings.ModelDisableDelay * animSpeedFactor);

			m_EHandler.ItemModelTransform.SetActive(false);

			yield return new WaitForSeconds((throwSettings.SpawnDelay - throwSettings.ModelDisableDelay) * animSpeedFactor);

			Vector3 position = transform.position + camera.transform.TransformVector(throwSettings.SpawnOffset);
			Quaternion rotation = camera.transform.rotation;

			Projectile projectile = Instantiate(m_ThrowingWeaponSettings.Projectile, position, rotation);

			Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();
			projectileRB.velocity = projectile.transform.forward * throwSettings.ThrowVelocity;
			projectileRB.angularVelocity = UnityEngine.Random.onUnitSphere * throwSettings.AngularSpeed;

			projectile.Launch(Player);

			if(m_ThrowingWeaponSettings.ObjectToSpawn != null)
				PoolingManager.Instance.GetObject(m_ThrowingWeaponSettings.ObjectToSpawn, transform.position + camera.transform.TransformVector(m_ThrowingWeaponSettings.ObjectToSpawnOffset), rotation, null);

			Player.DestroyEquippedItem.Try(0f);
		}

		#region Internal
		[Serializable]
		private class ThrowingWeaponGeneralModule
		{
			public Projectile Projectile = null;

			[Space]

			[Tooltip("e.g. the prefab of the grenade clip.")]
			public GameObject ObjectToSpawn = null;

			public Vector3 ObjectToSpawnOffset = Vector3.zero;
		}

		[Serializable]
		private class ThrowingSettings
		{
			[Range(0f, 100f)]
			public float ThrowVelocity = 15f;

			[Clamp(0f, 1000f)]
			public float AngularSpeed = 0f;

			[Space]

			public Vector3 SpawnOffset = Vector3.zero;

			[Range(0f, 3f)]
			public float AnimThrowSpeed = 1f;

			[Space]

			[Tooltip("Time to disable the mesh of the throwable")]
			public float ModelDisableDelay = 0.9f;

			[Tooltip("Time to spawn the projectile")]
			public float SpawnDelay = 1f;

			[Space]

			public DelayedCameraForce[] CameraForces = null;

			public DelayedSound[] ThrowAudio = null;
		}
        #endregion
    }
}