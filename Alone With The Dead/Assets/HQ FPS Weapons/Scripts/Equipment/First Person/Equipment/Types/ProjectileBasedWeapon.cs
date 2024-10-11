using UnityEngine;
using System;
using System.Collections;

namespace HQFPSWeapons
{
	/// <summary>
	/// Base class from which guns and any other projectile weapons derive from
	/// </summary>
	public abstract class ProjectileBasedWeapon : EquipmentItem
    {
		#region Internal
		[Flags]
		public enum FireMode
		{
			None = 0,
			Safety = 1,
			SemiAuto = 2,
			Burst = 4,
			FullAuto = 8,
			All = ~0
		}

		[Serializable]
		protected class FireModeSettings
		{
			[BHeader("Fire Mode")]

			public FireMode Modes = FireMode.SemiAuto;

			[Tooltip("How the fire rate will transform (in continuous use) on the duration of the magazine, the max x value(1) will be used if the whole magazine has been used")]
			public AnimationCurve FireRateOverTime = new AnimationCurve(
				new Keyframe(1f, 1f),
				new Keyframe(1f, 1f));

			[Tooltip("The minimum time that can pass between consecutive shots.")]
			public float FireDuration = 0.22f;

			[Space(3f)]

			[Tooltip("How many shots will the gun fire when in Burst-mode.")]
			public int BurstLength = 3;

			[Tooltip("How much time it takes to fire all the shots.")]
			public float BurstDuration = 0.3f;

			[Tooltip("The minimum time that can pass between consecutive bursts.")]
			public float BurstPause = 0.35f;

			[Space(3f)]

			[Tooltip("The maximum amount of shots that can be executed in a minute.")]
			public int RoundsPerMinute = 450;

			[Space(3f)]

			[BHeader("Animation", order = 100)]

			public bool HasDryFireAnim = false;

			public bool HasAlternativeFireAnim = false;

			[Tooltip("The minimum time that can pass between consecutive shots.")]
			[Range(0.1f, 5f)]
			public float FireAnimationSpeed = 1f;
		}

		[Serializable]
		protected struct CameraForcesModule
		{
			public DelayedCameraForce[] ReloadLoopCamForces;
			public DelayedCameraForce[] ReloadStartCamForces;
			public DelayedCameraForce[] ReloadEndCamForces;
			public DelayedCameraForce[] EmptyReloadLoopCamForces;
			public DelayedCameraForce[] HandlingCamForces;
		}

		[Serializable]
		protected struct ReloadSettings
		{
			[BHeader("Reload")]

			public Type ReloadType;

			[Tooltip("The time between reloading starts and the first bullet insert.")]
			[ShowIf("ReloadType", (int)Type.Progressive, 0f)]
			public float ReloadStartDuration;

			[Tooltip("How much time it takes to reload the gun.")]
			public float ReloadDuration;

			[ShowIf("ReloadType", (int)Type.Progressive, 0f)]
			public float ReloadEndDuration;

			[Space(2.5f)]

			[BHeader("Empty Reload", order = 100)]

			public bool HasEmptyReload;

			[Tooltip("How much time it takes to reload the gun and chamber the first bullet.")]
			[ShowIf("HasEmptyReload", true, 5f)]
			public float EmptyReloadDuration;

			[ShowIf("HasEmptyReload", true, 5f)]
			public bool ProgressiveEmptyReload;

			[BHeader("Animation")]

			[SerializeField]
			public float ReloadAnimationSpeed;

			[SerializeField]
			[ShowIf("HasEmptyReload", true, 5f)]
			public float EmptyReloadAnimationSpeed;

			// --------------------- Internal ---------------------
			public enum Type { Once, Progressive }
		}

		[Serializable]
		protected class WeaponAudioSetttings
		{
			[BHeader("General", true)]
			[Tooltip("Sounds that will play when firing the gun.")]
			public SoundPlayer FireSounds;

			public SoundPlayer FireTailSounds;

			[Group]
			[Tooltip("Sounds that will play after firing the gun.")]
			public DelayedSound[] HandlingSounds;

			public SoundPlayer AimSounds;

			[Space]

			[BHeader("Reload Sounds", order = 100)]

			[Group]
			public DelayedSound[] ReloadSounds;

			[Group]
			public DelayedSound[] ReloadStartSounds;

			[Group]
			public DelayedSound[] ReloadEndSounds;

			[Space(3)]

			[Group]
			public DelayedSound[] EmptyReloadSounds;

			[Space]

			[BHeader("Misc", order = 100)]
			public SoundPlayer EmptyGunSounds;

			[Group]
			public DelayedSound[] ShellDropSounds;

			[Group]
			public DelayedSound[] FireModeChangeSounds;
		}
		#endregion

		/// <summary>
		/// Raycast event, called when this weapon is used
		/// </summary>
		public Message<RaycastHit> WeaponShoot = new Message<RaycastHit>();

		public int SelectedFireMode { get; protected set; } = 2;
		public int ContinuouslyUsedTimes { get => continuouslyUsedTimes; }

		[BHeader("PROJECTILE WEAPON SETTINGS", true)]

		[SerializeField]
		[Group]
		protected FireModeSettings fireMode = null;

		[SerializeField]
		[Group]
		private ReloadSettings m_ReloadSettings = new ReloadSettings();

		[Space]

		[SerializeField]
		[Group]
		private CameraForcesModule m_CameraForces = new CameraForcesModule();

		[SerializeField]
		[Group]
		protected WeaponAudioSetttings m_GunAudio = null;

		// Firing
		protected int continuouslyUsedTimes = 0;

		// Aiming
		protected float nextTimeCanAim;

		// Reloading
		private bool m_ReloadLoopStarted;
		private float m_ReloadLoopEndTime;
		private float m_ReloadStartTime;
		private bool m_EndReload;

		// Caches for coroutine
		private WaitForSeconds m_BurstWait;
		private WaitForSeconds m_FireWait;

		private int m_CurrentFireAnimIndex = 0;


		public override bool TryUseOnce(Camera camera)
		{ 
			if ((Time.time < m_NextTimeCanUse || (m_Ammo.NeedAmmoToUse && CurrentAmmoInfo.Val.CurrentInMagazine == 0) || SelectedFireMode == (int)FireMode.Safety))
			{
				//Play Empty/Dry fire sound
				if ((CurrentAmmoInfo.Val.CurrentInMagazine == 0 || SelectedFireMode == (int)FireMode.Safety) && Time.time > m_NextTimeCanUse && !Player.Reload.Active)
				{
					m_EHandler.PlayAudio(m_GunAudio.EmptyGunSounds, 1f);

					if (fireMode.HasDryFireAnim)
					{
						m_EHandler.Animator.SetFloat("Fire Index", 4);
						m_EHandler.Animator.SetTrigger("Fire");
					}
				}

				return false;
			}

			//Shooting
			m_NextTimeCanUse = Time.time + (m_UseThreshold * Mathf.Clamp((1 / fireMode.FireRateOverTime.Evaluate(continuouslyUsedTimes / (float)m_Ammo.Settings.MagazineSize)), 0.1f, 10f));

			//Increment the 'm_ContinuouslyUsedTimes' variable, which shows how many times the weapon has been used without stopping
			if (m_EHandler.UsingItem.Active)
				continuouslyUsedTimes++;
			else
				continuouslyUsedTimes = 1;

			if (SelectedFireMode == (int)FireMode.Burst)
				StartCoroutine(C_DoBurst(camera));
			else
				Shoot(camera);

			return true;
		}

		public override bool TryUseContinuously(Camera camera)
		{
			//Used to prevent calling the "Play empty/dry fire functionality" 
			if (CurrentAmmoInfo.Val.CurrentInMagazine == 0 && NeedsAmmoToUse)
				return false;

			if(SelectedFireMode == (int)FireMode.FullAuto)
				return TryUseOnce(camera);

			return false;
		}

		public override bool CanAim()
		{
			return Time.time > nextTimeCanAim;
		}

		public override void OnAimStart()
		{
			base.OnAimStart();
			m_EHandler.PlayAudio(m_GunAudio.AimSounds, 1f);
		}

		public override void OnAimEnd()
		{
			base.OnAimEnd();
			m_EHandler.PlayAudio(m_GunAudio.AimSounds, 1f);
		}

        public override void OnUseEnd()
        {
			continuouslyUsedTimes = 0;
		}

        public virtual void Shoot(Camera camera)
		{
			if (CurrentAmmoInfo.Val.CurrentInMagazine <= 0 && NeedsAmmoToUse)
				return;

			// Fire sound
			m_EHandler.PlayAudio(m_GunAudio.FireSounds, 1f);

			// Handling sounds
			m_EHandler.PlaySounds(m_GunAudio.HandlingSounds);

			// Play Fire Animation 
			int fireIndex;

			if (!Player.Aim.Active)
			{
				fireIndex = m_CurrentFireAnimIndex == 0 ? 0 : 2;

				if(fireMode.HasAlternativeFireAnim)
					m_CurrentFireAnimIndex = m_CurrentFireAnimIndex == 0 ? 1 : 0;
			}
			else
			{
				fireIndex = m_CurrentFireAnimIndex == 0 ? 1 : 3;

				if (fireMode.HasAlternativeFireAnim)
					m_CurrentFireAnimIndex = m_CurrentFireAnimIndex == 0 ? 1 : 0;
			}

			m_EHandler.Animator.SetFloat("Fire Index", fireIndex);
			m_EHandler.Animator.SetFloat("Fire Speed", fireMode.FireAnimationSpeed);
			m_EHandler.Animator.SetTrigger("Fire");


			// Cam Forces
			m_EHandler.PlayCameraForces(m_CameraForces.HandlingCamForces);

			// Ammo
			if (m_Ammo.NeedAmmoToUse)
			{
				m_AmmoProperty.AdjustValue(-1, 0, m_Ammo.Settings.MagazineSize);
				UpdateAmmoInfo();

				if ((int)m_AmmoProperty.Val.Current == 0 && m_EHandler.EquipmentManager.AutoReload)
					StartCoroutine(C_StartReloadDelayed());
			}
		}

		public override bool TryStartReloading()
		{
			if (Time.time > m_NextTimeCanUse && m_ReloadLoopEndTime < Time.time && m_Ammo.NeedAmmoToUse && CurrentAmmoInfo.Val.CurrentInMagazine < m_Ammo.Settings.MagazineSize)
			{
				m_EHandler.ClearDelayedCamForces();
				m_EHandler.ClearDelayedSounds();

				m_AmmoToAdd = m_Ammo.Settings.MagazineSize - CurrentAmmoInfo.Val.CurrentInMagazine;

				if (CurrentAmmoInfo.Val.CurrentInStorage < m_AmmoToAdd)
					m_AmmoToAdd = CurrentAmmoInfo.Val.CurrentInStorage;

				if (m_AmmoToAdd > 0)
				{
					if (CurrentAmmoInfo.Val.CurrentInMagazine == 0 && m_ReloadSettings.HasEmptyReload)
					{
						//Empty/Dry Reload
						m_EHandler.Animator.SetFloat("Empty Reload Speed", m_ReloadSettings.EmptyReloadAnimationSpeed);

						if (m_ReloadSettings.ReloadType == ReloadSettings.Type.Once)
						{
							m_ReloadLoopEndTime = Time.time + m_ReloadSettings.EmptyReloadDuration;
							m_EHandler.Animator.SetTrigger("Empty Reload");

							m_EHandler.PlayCameraForces(m_CameraForces.EmptyReloadLoopCamForces);
							m_EHandler.PlaySounds(m_GunAudio.EmptyReloadSounds);
						}
						else if (m_ReloadSettings.ReloadType == ReloadSettings.Type.Progressive)
						{
							m_ReloadStartTime = Time.time + m_ReloadSettings.EmptyReloadDuration;
							m_EHandler.Animator.SetTrigger("Empty Reload");

							m_EHandler.PlayCameraForces(m_CameraForces.EmptyReloadLoopCamForces);
							m_EHandler.PlaySounds(m_GunAudio.EmptyReloadSounds);
						}
					}
					else
					{
						//Tactical Reload
						m_EHandler.Animator.SetFloat("Reload Speed", m_ReloadSettings.ReloadAnimationSpeed);

						if (m_ReloadSettings.ReloadType == ReloadSettings.Type.Once)
						{
							m_ReloadLoopEndTime = Time.time + m_ReloadSettings.ReloadDuration;
							m_EHandler.Animator.SetTrigger("Reload");

							m_EHandler.PlayCameraForces(m_CameraForces.ReloadLoopCamForces);
							m_EHandler.PlaySounds(m_GunAudio.ReloadSounds);
						}
						else if (m_ReloadSettings.ReloadType == ReloadSettings.Type.Progressive)
						{
							m_ReloadStartTime = Time.time + m_ReloadSettings.ReloadStartDuration;
							m_EHandler.Animator.SetTrigger("Start Reload");

							m_EHandler.PlayCameraForces(m_CameraForces.ReloadStartCamForces);
							m_EHandler.PlaySounds(m_GunAudio.ReloadStartSounds);
						}
					}

					if(m_ReloadSettings.ReloadType == ReloadSettings.Type.Once)
						UpdateAmmoInfo();

					return true;
				}
			}

			return false;
		}

		//This method is called by the 'Equipment Manager' to check if the reload is finished
		public override bool IsDoneReloading()
		{
			if (!m_ReloadLoopStarted)
			{
				if (Time.time > m_ReloadStartTime)
				{
					if (CurrentAmmoInfo.Val.CurrentInMagazine == 0 && m_ReloadSettings.HasEmptyReload)
					{
						//'Empty/Dry Reload'
						m_ReloadLoopStarted = true;

						if (m_ReloadSettings.ProgressiveEmptyReload && m_ReloadSettings.ReloadType == ReloadSettings.Type.Progressive)
						{
							if (m_AmmoToAdd > 1)
							{
								//Play the reload start State after the empty reload
								m_EHandler.PlayCameraForces(m_CameraForces.ReloadStartCamForces);
								m_EHandler.PlaySounds(m_GunAudio.ReloadStartSounds);

								m_ReloadLoopEndTime = Time.time + m_ReloadSettings.ReloadStartDuration;
								m_EHandler.Animator.SetTrigger("Start Reload");
							}
							else
							{
								GetAmmoFromInventory(1);

								m_AmmoProperty.AdjustValue(1, 0, m_Ammo.Settings.MagazineSize);
								m_AmmoToAdd--;

								UpdateAmmoInfo();

								return true;
							}
						}
					}
					else
					{
						//'Tactical Reload'
						m_ReloadLoopStarted = true;
						m_ReloadLoopEndTime = Time.time + m_ReloadSettings.ReloadDuration;

						m_EHandler.PlayCameraForces(m_CameraForces.ReloadLoopCamForces);
						m_EHandler.PlaySounds(m_GunAudio.ReloadSounds);

						m_EHandler.Animator.SetTrigger("Reload");
					}
				}

				return false;
			}

			if (m_ReloadLoopStarted && Time.time >= m_ReloadLoopEndTime)
			{
				if (m_ReloadSettings.ReloadType == ReloadSettings.Type.Once || (CurrentAmmoInfo.Val.CurrentInMagazine == 0 && !m_ReloadSettings.ProgressiveEmptyReload))
				{
					m_AmmoProperty.AdjustValue(m_AmmoToAdd, 0, m_Ammo.Settings.MagazineSize);
					GetAmmoFromInventory(m_AmmoToAdd);

					m_AmmoToAdd = 0;
				}
				else if (m_ReloadSettings.ReloadType == ReloadSettings.Type.Progressive)
				{
					if (m_AmmoToAdd > 0)
					{
						GetAmmoFromInventory(1);

						m_AmmoProperty.AdjustValue(1, 0, m_Ammo.Settings.MagazineSize);
						m_AmmoToAdd--;
					}

					if (m_AmmoToAdd > 0)
					{
						m_EHandler.PlayCameraForces(m_CameraForces.ReloadLoopCamForces);
						m_EHandler.PlaySounds(m_GunAudio.ReloadSounds);

						m_EHandler.Animator.SetTrigger("Reload");
						m_ReloadLoopEndTime = Time.time + m_ReloadSettings.ReloadDuration;
					}
					else if (!m_EndReload)
					{
						m_EHandler.Animator.SetTrigger("End Reload");
						m_EndReload = true;
						m_ReloadLoopEndTime = Time.time + m_ReloadSettings.ReloadEndDuration;

						m_EHandler.PlayCameraForces(m_CameraForces.ReloadEndCamForces);
						m_EHandler.PlaySounds(m_GunAudio.ReloadEndSounds);
					}
					else
						m_EndReload = false;
				}

				UpdateAmmoInfo();

				return !m_EndReload && m_AmmoToAdd == 0;
			}

			return false;
		}

		public override void OnReloadEnd()
		{
			m_ReloadLoopEndTime = Time.time;
			m_EndReload = false;
			m_ReloadLoopStarted = false;

			m_EHandler.ClearDelayedSounds();
			m_EHandler.ClearDelayedCamForces();
		}

		protected virtual void Start()
		{
			if (fireMode != null)
			{
				m_BurstWait = new WaitForSeconds(fireMode.BurstDuration / fireMode.BurstLength);
				m_FireWait = new WaitForSeconds(fireMode.FireDuration + 0.3f);

				if (SelectedFireMode == (int)FireMode.SemiAuto)
					m_UseThreshold = fireMode.FireDuration;
				else if (SelectedFireMode == (int)FireMode.Burst)
					m_UseThreshold = fireMode.BurstDuration + fireMode.BurstPause;
				else
					m_UseThreshold = 60f / fireMode.RoundsPerMinute;
			}
		}

		private void OnValidate()
		{
			Start();
		}

		private IEnumerator C_DoBurst(Camera camera)
		{
			for (int i = 0; i < fireMode.BurstLength; i++)
			{
				Shoot(camera);
				yield return m_BurstWait;
			}
		}

		private IEnumerator C_StartReloadDelayed()
		{
			yield return m_FireWait;

			Player.Reload.TryStart();
		}

		public override float GetTimeBetweenUses() 
		{
			if (SelectedFireMode == (int)FireMode.FullAuto)
				return m_UseThreshold * Mathf.Clamp((1 / fireMode.FireRateOverTime.Evaluate(continuouslyUsedTimes / (float)m_Ammo.Settings.MagazineSize)), 0.1f, 10f);
			else
				return m_UseThreshold / 10f;
		}
	}
}
