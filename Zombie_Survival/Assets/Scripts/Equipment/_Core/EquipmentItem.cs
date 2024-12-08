using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	/// <summary>
	/// An object which can be used by a character.
	/// Base class from which weapon and tool scripts derive.
	/// </summary>
	public abstract class EquipmentItem : FPEquipmentComponent
	{
		#region Internal
		[Serializable]
		private class GeneralSettings
		{
			[Range(1, 180)]
			public float FieldOfView = 45f;

			public bool EnableAiming = false;

			[ShowIf("EnableAiming", true)]
			public bool UseAimBlur = false;

			public bool UseWhileAirborne = false;

			public bool CanStopReloading = false;

			[Range(0f, 35f)]
			public float StaminaTakePerUse = 0f;
		}

		[Serializable]
		private class AudioSettings
		{
			public DelayedSound[] EquipSounds = null;

			public SoundPlayer UnequipSounds = null;
		}

		[Serializable]
		private class AnimationSettings
		{
			[Range(0.1f, 3f)]
			public float EquipSpeed = 1f;

			[Range(0.1f, 3f)]
			public float EquipTime = 0.6f;

			[Space(3f)]

			[Range(0.1f, 3f)]
			public float UnequipSpeed = 1f;

			[Range(0.1f, 3f)]
			public float UnequipTime = 0.5f;
		}

		[Serializable]
		private class CameraSettings
		{
			public CameraHeadBob AimCamHeadbob = null;

			[Space]

			public DelayedCameraForce[] EquipCamForces = null;
			public DelayedCameraForce[] UnequipCamForces = null;
		}

		#pragma warning disable 0649
		[Serializable]
		protected struct AmmoSettingsModule
		{
			public bool NeedAmmoToUse;

			[ShowIf("NeedAmmoToUse", true, 3f)]
			public AmmoSettings Settings;

			[Serializable]
			public struct AmmoSettings
			{
				[DatabaseItem]
				public string AmmoItem;

				[Clamp(0, 100)]
				public int MagazineSize;

				[Clamp(0, 1000)]
				public int StorageSize;
			}
		}

		public struct AmmoInfo
		{
			public int CurrentInMagazine;
			public int CurrentInStorage;
		}
		#endregion

		//Events
		public Value<AmmoInfo> CurrentAmmoInfo = new Value<AmmoInfo>();

		//Properties
		public bool EnableAiming { get { return m_GeneralSettings.EnableAiming; } protected set { m_GeneralSettings.EnableAiming = value; } }
		public float FieldOfView { get => m_GeneralSettings.FieldOfView; }
		public float WieldTime { get => m_BaseAnimation.EquipTime; }
		public float UnWieldtime { get => m_BaseAnimation.UnequipTime; }
		public float StaminaTakePerUse { get => m_GeneralSettings.StaminaTakePerUse; }
		public bool UseAimBlur { get => m_GeneralSettings.UseAimBlur; }
		public bool UseWhileAirborne { get => m_GeneralSettings.UseWhileAirborne; }
		public bool CanStopReloading { get => m_GeneralSettings.CanStopReloading; }
		public bool NeedsAmmoToUse { get => m_Ammo.NeedAmmoToUse; }
		public bool UseClickBuffer { get => m_UseClickBuffer; }
		public CameraHeadBob AimCamHeadbob { get => m_BaseCamera.AimCamHeadbob; }

		[BHeader("EQUIPMENT ITEM SETTINGS", true)]

		[SerializeField]
		[Group]
		private GeneralSettings m_GeneralSettings = null;

		[Space]

		[SerializeField]
		[Group]
		private AnimationSettings m_BaseAnimation = null;

		[SerializeField]
		[Group]
		private AudioSettings m_BaseAudio = null;

		[SerializeField]
		[Group]
		private CameraSettings m_BaseCamera = null;

		[Space]

		[SerializeField]
		[Group]
		protected AmmoSettingsModule m_Ammo = new AmmoSettingsModule();

		protected float m_UseThreshold = 0.1f;
		protected float m_NextTimeCanUse;

		// Cache some properties of the item
		private ItemProperty.Value m_Durability;
		protected ItemProperty.Value m_AmmoProperty;

		// Reloading
		protected int m_AmmoToAdd;

		// Misc
		protected bool m_UseClickBuffer = false;


		public virtual bool TryUseOnce(Camera camera) { return false; }
		public virtual bool TryUseContinuously(Camera camera) { return false; }
		public virtual bool TryStartReloading() { return false; }
		public virtual bool IsDoneReloading() { return false; }
		public virtual void OnReloadEnd() { }
		public virtual bool CanAim() { return true; }
		public virtual void OnAimStart() { m_EHandler.Animator.SetInteger("Patrol Index", 0); }
		public virtual void OnAimEnd() { m_EHandler.Animator.SetInteger("Patrol Index", 1); }
		public virtual bool ChangeFireMode() { return false; }
		public virtual float GetTimeBetweenUses() { return m_UseThreshold; }
		public virtual void OnUseStart() { }
		public virtual void OnUseEnd() { }

		public virtual void Wield(SaveableItem item)
		{
			if (m_EHandler.Animator != null)
			{
				m_EHandler.Animator.SetFloat("Wield Speed", m_BaseAnimation.EquipSpeed);
				m_EHandler.Animator.Play("Wield");
			}

			m_AmmoProperty = m_EHandler.CurrentlyAttachedItem.GetProperty("Ammo");

			if (m_Ammo.NeedAmmoToUse)
			{
				if (m_AmmoProperty != null)
				{
					m_AmmoProperty.AdjustValue(0f, 0f, m_Ammo.Settings.MagazineSize);
					UpdateAmmoInfo();
				}
				else
					Debug.LogError("Wieldable with game object name '" + name + "' has ammo enabled but no ammo property found on the item.");
			}

			m_EHandler.PlayCameraForces(m_BaseCamera.EquipCamForces);
			m_EHandler.PlaySounds(m_BaseAudio.EquipSounds);
		}

		public virtual void Unwield()
		{
			m_EHandler.EquipmentManager.PlayPersistentAudio(m_BaseAudio.UnequipSounds, 1f, ItemSelection.Method.RandomExcludeLast);
			m_EHandler.PlayCameraForces(m_BaseCamera.UnequipCamForces);

			if (m_EHandler.Animator != null)
			{
				m_EHandler.Animator.SetTrigger("Unwield");
				m_EHandler.Animator.SetFloat("Unwield Speed", m_BaseAnimation.UnequipSpeed);
			}
		}

		public void UpdateAmmoInfo()
		{
			if (!m_Ammo.NeedAmmoToUse)
				return;

			CurrentAmmoInfo.Set(
				new AmmoInfo()
				{
					CurrentInMagazine = (int)m_AmmoProperty.Val.Current,

					// Get the ammo count from the inventory
					CurrentInStorage = Mathf.Clamp(GetAmmoCount(), 0, m_Ammo.Settings.StorageSize)
				});
		}

		public int TryGetMagazineSize()
		{
			if (m_EHandler.CurrentlyAttachedItem != null)
				return m_Ammo.Settings.MagazineSize;
			else return 100;
		}

		protected int GetAmmoCount()
		{
			return Player.Inventory.GetItemCount(m_Ammo.Settings.AmmoItem);
		}

		protected int GetAmmoFromInventory(int amount)
		{
			return Player.Inventory.RemoveItems(m_Ammo.Settings.AmmoItem, amount, ItemContainerFlags.AmmoPouch);
		}

		protected int AddAmmoToInventory(int amount)
		{
			return Player.Inventory.AddItem(m_Ammo.Settings.AmmoItem, amount, ItemContainerFlags.Storage);
		}
    }

	[Serializable]
	public class DelayedCameraForce : CloneableObject<DelayedCameraForce>
	{
		public float Delay = 0f;

		public SpringForce Force = new SpringForce();
	}

	[Serializable]
	public class DelayedSound
	{
		public SoundPlayer Sound;

		public Vector2 DelayRange;

		public float GetDelay()
		{
			if (DelayRange.x == DelayRange.y)
				return DelayRange.x;
			else
				return Random.Range(Mathf.Abs(DelayRange.x), Mathf.Abs(DelayRange.y));
		}
	}

	[Serializable]
	public class QueuedCameraForce
	{
		public DelayedCameraForce DelayedForce { get; private set; }
		public float PlayTime { get; private set; }

		public QueuedCameraForce(DelayedCameraForce force, float playTime)
		{
			DelayedForce = force;
			PlayTime = playTime;
		}
	}

	public class QueuedSound
	{
		public DelayedSound DelayedSound { get; private set; }
		public float PlayTime { get; private set; }

		public QueuedSound(DelayedSound clip, float playTime)
		{
			DelayedSound = clip;
			PlayTime = playTime;
		}
	}
}