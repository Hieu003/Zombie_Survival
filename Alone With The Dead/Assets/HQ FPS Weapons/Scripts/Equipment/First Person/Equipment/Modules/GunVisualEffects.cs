using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	public class GunVisualEffects : FPEquipmentComponent
	{
		#region Internal
		#pragma warning disable 0649
		[Serializable]
		private struct VisualEffectsSettings
		{
			[BHeader("Muzzle Flash")]
			public GameObject MuzzleFlashPrefab;

			[Space]

			public Vector3 MuzzleFlashOffset;

			public Vector2 MuzzleFlashRandomScale;

			public Vector3 MuzzleFlashRandomRot;

			[BHeader("Tracer")]

			public GameObject TracerPrefab;

			[Space]

			public Vector3 TracerOffset;

			[BHeader("Light")]

			public Vector3 LightOffset;
		}

		[Serializable]
		private class CasingEjectionSettings
		{
			[BHeader("Casing Ejection")]

			public GameObject CasingPrefab;

			[Space]

			public float SpawnDelay;

			public float CasingScale = 1f;

			public float Spin;

			public Vector3 SpawnOffset;

			public Vector3 SpawnVelocity;
		}

		[Serializable]
		private class MagazineEjectionSettings
		{
			[BHeader("Magazine Ejection")]

			public GameObject MagazinePrefab;

			[Space]

			public float SpawnDelay;

			public Vector3 MagazineOffset;

			public Vector3 MagazineAngularVelocity;
		}
		#endregion

		[SerializeField]
		[Group]
		private VisualEffectsSettings m_ParticleEffects = new VisualEffectsSettings();

		[SerializeField]
		[Group]
		private CasingEjectionSettings m_CasingEjection = null;

		[SerializeField]
		[Group]
		private MagazineEjectionSettings m_MagazineEjection = null;

		private ProjectileBasedWeapon m_Weapon;

		private WaitForSeconds m_CasingSpawnDelay;
		private WaitForSeconds m_MagazineSpawnDelay;

		private EquipmentHandler m_EquipHandler;
		private float m_NextTimeCanSpawnVfx = 0f;


		public override void Initialize(EquipmentHandler equipmentHandler)
		{
			base.Initialize(equipmentHandler);

			m_EquipHandler = equipmentHandler as EquipmentHandler;
			m_EquipHandler.EquipSettings.LightEffect.transform.localPosition = m_EquipHandler.EquipSettings.OriginalLightPosition + m_ParticleEffects.LightOffset;
		}

		private void Start()
		{
			m_Weapon = m_EHandler.CurrentItem as ProjectileBasedWeapon;

			m_CasingSpawnDelay = new WaitForSeconds(m_CasingEjection.SpawnDelay);
			m_MagazineSpawnDelay = new WaitForSeconds(m_MagazineEjection.SpawnDelay);

			m_Weapon.WeaponShoot.AddListener(SpawnEffects);
			Player.Reload.AddStartListener(SpawnMagazine);

			// Create a pool for each gun effect, to help performance
			int minPoolSize = m_Weapon.TryGetMagazineSize() * 2;
			int maxPoolSize = minPoolSize * 2;

			if (m_ParticleEffects.MuzzleFlashPrefab != null)
				PoolingManager.Instance.CreatePool(m_ParticleEffects.MuzzleFlashPrefab, minPoolSize, maxPoolSize, true, m_ParticleEffects.MuzzleFlashPrefab.GetInstanceID().ToString(), 1f);

			if (m_ParticleEffects.TracerPrefab != null)
				PoolingManager.Instance.CreatePool(m_ParticleEffects.TracerPrefab, minPoolSize, maxPoolSize, true, m_ParticleEffects.TracerPrefab.GetInstanceID().ToString(), 3f);

			if (m_MagazineEjection.MagazinePrefab != null)
				PoolingManager.Instance.CreatePool(m_MagazineEjection.MagazinePrefab, 3, 10, true, m_MagazineEjection.MagazinePrefab.GetInstanceID().ToString(), 10f);

			if (m_CasingEjection.CasingPrefab != null)
				PoolingManager.Instance.CreatePool(m_CasingEjection.CasingPrefab, minPoolSize, maxPoolSize, true, m_CasingEjection.CasingPrefab.GetInstanceID().ToString(), 5f);
		}

		private void OnValidate()
		{
			m_CasingSpawnDelay = new WaitForSeconds(m_CasingEjection.SpawnDelay);
			m_MagazineSpawnDelay = new WaitForSeconds(m_MagazineEjection.SpawnDelay);
		}

        private void OnDestroy()
        {
			m_Weapon.WeaponShoot.RemoveListener(SpawnEffects);
			Player.Reload.RemoveStartListener(SpawnMagazine);
		}

        private void SpawnMagazine() 
		{
			// Create the magazine if a prefab is assigned & if the the gun uses bullets
			if (m_MagazineEjection.MagazinePrefab != null && m_EquipHandler.EquipSettings.MagazineEjection != null)
			{
				StartCoroutine(C_SpawnMagazine());
			}
		}

		private void SpawnEffects(RaycastHit hitInfo)
		{
			if (gameObject.activeSelf == false)
				return;

			// Create the tracer if a prefab is assigned
			if (m_ParticleEffects.TracerPrefab != null && m_EquipHandler.EquipSettings.Muzzle != null)
			{
				PoolingManager.Instance.GetObject(
					m_ParticleEffects.TracerPrefab,
					m_EquipHandler.EquipSettings.Muzzle.position + m_EquipHandler.EquipSettings.Muzzle.TransformVector(m_ParticleEffects.TracerOffset),
					Quaternion.LookRotation(hitInfo.point - Player.Camera.transform.position)
				);
			}

			//Useful for weapons like shotguns, using multiple raycasts at the same time.
			if (m_NextTimeCanSpawnVfx > Time.time)
				return;

			// Muzzle flash
			if (m_ParticleEffects.MuzzleFlashPrefab != null && m_EquipHandler.EquipSettings.Muzzle != null)
			{
				var muzzleFlash = PoolingManager.Instance.GetObject(
					m_ParticleEffects.MuzzleFlashPrefab,
					Vector3.zero,
					Quaternion.identity,
					m_EquipHandler.EquipSettings.Muzzle
				);

				muzzleFlash.transform.localPosition = m_ParticleEffects.MuzzleFlashOffset;

				var randomMuzzleFlashRot = m_ParticleEffects.MuzzleFlashRandomRot;

				randomMuzzleFlashRot = new Vector3(
					Random.Range(-randomMuzzleFlashRot.x, randomMuzzleFlashRot.x),
					Random.Range(-randomMuzzleFlashRot.y, randomMuzzleFlashRot.y),
					Random.Range(-randomMuzzleFlashRot.z, randomMuzzleFlashRot.z));

				muzzleFlash.transform.localRotation = Quaternion.Euler(randomMuzzleFlashRot);

				float randomMuzzleFlashScale = Random.Range(m_ParticleEffects.MuzzleFlashRandomScale.x, m_ParticleEffects.MuzzleFlashRandomScale.y);

				muzzleFlash.transform.localScale = new Vector3(randomMuzzleFlashScale, randomMuzzleFlashScale, randomMuzzleFlashScale);
			}

			m_NextTimeCanSpawnVfx = Time.time + 0.05f;

			// Light
			if (m_EquipHandler.EquipSettings.LightEffect != null && m_EquipHandler.EquipSettings.Muzzle != null)
				m_EquipHandler.EquipSettings.LightEffect.Play(false);

			// Spawn the shell if a prefab is assigned
			if (m_CasingEjection.CasingPrefab != null)
				StartCoroutine(C_SpawnCasing());
		}

		private IEnumerator C_SpawnMagazine() 
		{
			yield return m_MagazineSpawnDelay;

			PoolableObject magazine = PoolingManager.Instance.GetObject(
				m_MagazineEjection.MagazinePrefab,
				m_EquipHandler.EquipSettings.MagazineEjection.position + m_EquipHandler.EquipSettings.MagazineEjection.TransformVector(m_MagazineEjection.MagazineOffset),
				Quaternion.identity);

			magazine.GetComponent<Rigidbody>().AddRelativeTorque(m_MagazineEjection.MagazineAngularVelocity);
		}

		private IEnumerator C_SpawnCasing()
		{
			if (m_EquipHandler.EquipSettings.CasingEjection == null)
				yield break;

			yield return m_CasingSpawnDelay;

			Quaternion cassingSpawnRotation = Quaternion.Euler(Random.Range(-30, 30), Random.Range(-30, 30), Random.Range(-30, 30));

			Vector3 cassingSpawnPosition = m_EquipHandler.EquipSettings.CasingEjection.TransformVector(m_CasingEjection.SpawnOffset);

			var cassing = PoolingManager.Instance.GetObject(m_CasingEjection.CasingPrefab.gameObject, m_EquipHandler.EquipSettings.CasingEjection.position + cassingSpawnPosition, cassingSpawnRotation);
			cassing.transform.localScale = new Vector3(m_CasingEjection.CasingScale, m_CasingEjection.CasingScale, m_CasingEjection.CasingScale);

			var cassingRB = cassing.GetComponent<Rigidbody>();

			cassingRB.maxAngularVelocity = 10000f;

			cassingRB.velocity = transform.TransformVector(new Vector3(
				m_CasingEjection.SpawnVelocity.x * Random.Range(0.75f, 1.15f),
				m_CasingEjection.SpawnVelocity.y * Random.Range(0.9f, 1.1f),
				m_CasingEjection.SpawnVelocity.z * Random.Range(0.85f, 1.15f))) + Player.Velocity.Get();

			cassingRB.angularVelocity = new Vector3(
				Random.Range(-m_CasingEjection.Spin, m_CasingEjection.Spin),
				Random.Range(-m_CasingEjection.Spin, m_CasingEjection.Spin),
				Random.Range(-m_CasingEjection.Spin, m_CasingEjection.Spin));
		}
	}
}
