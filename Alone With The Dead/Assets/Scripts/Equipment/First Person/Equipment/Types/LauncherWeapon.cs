using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles Weapons with non-Raycast Projectiles like Rocket Launchers , Bows, etc.
/// </summary>
namespace HQFPSWeapons
{
	public class LauncherWeapon : ProjectileBasedWeapon
	{
		#region Internal
		[Serializable]
		public class LauncherSettings
		{
			[Range(0f, 10f)]
			public float ProjectileSpread = 1.5f;

			[Range(0f, 3f)]
			public float AimSpreadFactor = 0.8f;

			[Range(0f, 3f)]
			public float CrouchSpreadFactor = 0.95f;

			[Range(0f, 3f)]
			public float WalkSpreadFactor = 0.95f;

			[Range(0f, 3f)]
			public float RunSpreadFactor = 0.95f;

			[Range(0f, 3f)]
			public float JumpSpreadFactor = 0.95f;
		}

		[Serializable]
		public class ProjectileSettings
		{
			public ShaftedProjectile Prefab;

			[Range(0f, 100f)]
			public float LaunchSpeed = 15f;

			public Vector3 AngularVelocity;

			public Vector3 SpawnOffset;

			[Range(0, 5f)]
			public float SpawnDelay = 0.3f;
		}
		#endregion

		[BHeader("LAUNCHER WEAPON", true)]

		[SerializeField]
		[Group]
		private LauncherSettings m_Launcher = null;

		[SerializeField]
		[Group]
		private ProjectileSettings m_Projectile = null;

		private WaitForSeconds m_WaitAfterLaunch;


		public override void Shoot(Camera camera)
		{
			base.Shoot(camera);

			StartCoroutine(C_LaunchWithDelay(camera));
		}

		protected override void Start()
		{
			base.Start();

			m_WaitAfterLaunch = new WaitForSeconds(m_Projectile.SpawnDelay);
		}

		private IEnumerator C_LaunchWithDelay(Camera camera) 
		{
			yield return m_WaitAfterLaunch;

			if (!m_Projectile.Prefab)
			{
				Debug.LogErrorFormat("No Projectile prefab assigned in the inspector! Please assign one.");
				yield return null;
			}

			Ray ray = camera.ViewportPointToRay(Vector3.one * 0.5f);
			float spread = m_Launcher.ProjectileSpread;

			if (Player.Jump.Active)
				spread *= m_Launcher.JumpSpreadFactor;
			else if (Player.Run.Active)
				spread *= m_Launcher.RunSpreadFactor;
			else if (Player.Crouch.Active)
				spread *= m_Launcher.CrouchSpreadFactor;
			else if (Player.Walk.Active)
				spread *= m_Launcher.WalkSpreadFactor;

			if (Player.Aim.Active)
				spread *= m_Launcher.AimSpreadFactor;

			Vector3 spreadVector = camera.transform.TransformVector(new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0f));
			ray.direction = Quaternion.Euler(spreadVector) * ray.direction;

			Vector3 position = transform.position + camera.transform.TransformVector(m_Projectile.SpawnOffset);
			Quaternion rotation = Quaternion.LookRotation(ray.direction);

			ShaftedProjectile projectileObject = Instantiate(m_Projectile.Prefab, position, rotation);
			projectileObject.GetComponent<Rigidbody>().velocity = projectileObject.transform.forward * m_Projectile.LaunchSpeed;
			projectileObject.GetComponent<ShaftedProjectile>().Launch(Player);
			projectileObject.GetComponent<ShaftedProjectile>().CheckForSurfaces(camera.transform.position, camera.transform.forward);

			RaycastHit hitInfo = new RaycastHit();

			WeaponShoot.Send(hitInfo);
		}
    }
}
