using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HQFPSWeapons
{
	public class EquipmentManager : PlayerComponent
	{
		#region Internal
		[Serializable]
		public struct EquippableCategories
		{
			[DatabaseCategory]
			public string Category;

			[Range(0.1f, 2f)]
			public float MovementSpeedMultiplier;
		}
		#endregion

		public Camera WorldCamera => m_WorldCamera.UnityCamera;
		public Camera ViewCamera => m_ViewCamera;
		public MouseLook MouseLook => m_Mouselook;
		public bool AutoReload => m_AutoReload;

		[SerializeField]
		private FirstPersonCamera m_WorldCamera = null;

		[SerializeField]
		private Camera m_ViewCamera = null;

		[SerializeField]
		private MouseLook m_Mouselook = null;

		[Space]

		[SerializeField]
		private bool m_ReloadWhileRunning = false;

		[SerializeField]
		private bool m_AimWhileReloading = false;

		[Space]

		[SerializeField]
		private bool m_AutoReload = true;

		[SerializeField]
		private bool m_ReloadIfDryFire = true;

		[Space]

		[SerializeField]
		private int m_ArmsIndex = 0;

		[Space]

		[SerializeField]
		[Range(0f, 100f)]
		private float m_FOVSetSpeed = 30f;

		[SerializeField]
		[Range(0f, 120f)]
		private float m_NormalFOV = 75f;

		[SerializeField]
		[Range(0f, 120f)]
		private float m_AimFOV = 45f;

		[SerializeField]
		[Range(0.1f, 1f)]
		private float m_AimPlayerSpeedMultiplier = 0.8f;

		[Space]

		[SerializeField]
		private EquipmentHandler[] m_EquipmentHandlers = null;

		[SerializeField]
		private EquippableCategories[] m_EquipableCategories = null;

		private AudioSource m_PersistentAudioSource;

		private float m_NextTimeCanWield;
		private bool m_WaitingToWield;

		private Coroutine m_FOVSetter;

		private Dictionary<string, float> m_EquipableItemsCategories = new Dictionary<string, float>();


		public void OnLoad()
		{
			HoldItem(Player.EquippedItem.Get(), true);
		}

		public void PlayPersistentAudio(SoundPlayer soundPlayer, float volume, ItemSelection.Method selectionMethod = ItemSelection.Method.RandomExcludeLast)
		{
			soundPlayer.Play(selectionMethod, m_PersistentAudioSource, volume);
		}

		public void PlayPersistentAudio(AudioClip clip, float volume)
		{
			m_PersistentAudioSource.PlayOneShot(clip, volume);
		}

		private void Awake()
		{
			m_WorldCamera.UnityCamera.fieldOfView = m_NormalFOV;
			m_WorldCamera.AimHeadBob = null;

			//Persistent AudioSource (For Fire Tail Sounds)
			m_PersistentAudioSource = AudioUtils.CreateAudioSource("Persistent Audio Src", transform, Vector3.zero, false, 1f, 2.5f);
			m_PersistentAudioSource.bypassEffects = m_PersistentAudioSource.bypassListenerEffects = m_PersistentAudioSource.bypassReverbZones = false;

			Player.EquipItem.SetTryer(Try_HoldItem);
			Player.DestroyEquippedItem.SetTryer(Try_DestroyHeldItem);

			Player.SwapItems.SetTryer(Try_SwapItems);
			Player.ItemIsSwappable.SetTryer(IsSwappable);

			Player.ChangeFireMode.SetTryer(Try_ChangeFireMode);

			Player.ChangeArms.SetTryer(Try_ChangeArms);

			Player.UseOnce.SetTryer(() => Try_Use(false));
			Player.UseContinuously.SetTryer(() => Try_Use(true));

			Player.Aim.AddStartTryer(TryStart_Aim);
			Player.Aim.AddStopListener(OnStop_Aim);

			Player.Reload.AddStartTryer(TryStart_Reload);
			Player.Reload.AddStopListener(OnStop_Reload);

			Player.ObjectInProximity.AddChangeListener(OnChanged_IsCloseToAnObject);

			InitiateEquippableItems();

			foreach (var handler in m_EquipmentHandlers)
				handler.UpdateFirstPersonArms(m_ArmsIndex);
		}

		private void Update()
		{
			if (Player.Reload.Active)
			{
				if (!m_ReloadWhileRunning && Player.Run.Active)
					Player.Reload.ForceStop();
				else if (m_EquipmentHandlers[0].CurrentItem == null)
					Debug.LogError("The Reload activity is active but no wieldable is enabled.");
				else
				{
					bool endedReloading = m_EquipmentHandlers[0].CurrentItem.IsDoneReloading();

					if (endedReloading)
						Player.Reload.ForceStop();
				}
			}

			if (m_WaitingToWield && Time.time > m_NextTimeCanWield)
			{
				Wield();
				m_WaitingToWield = false;
			}
		}

		private bool Try_ChangeFireMode()
		{
			if (Player.Reload.Active || Player.Run.Active || Player.ActiveEquipmentItem.Val == null)
				return false;

			return Player.ActiveEquipmentItem.Get().ChangeFireMode();
		}

		private bool Try_HoldItem(SaveableItem item, bool instantly)
		{
			if (Player.EquippedItem.Get() == item)
				return false;

			HoldItem(item, instantly);

			return true;
		}

		private bool Try_ChangeArms()
		{
			if (FirstPersonArms.Default.GetFirstPersonArmsCount() < 2)
				return false;

			if (m_ArmsIndex >= FirstPersonArms.Default.GetFirstPersonArmsCount() - 1)
				m_ArmsIndex = 0;
			else
			{
				m_ArmsIndex += 1;
			}

			m_EquipmentHandlers[0].UpdateFirstPersonArms(m_ArmsIndex);

			return true;
		}

		private void HoldItem(SaveableItem item, bool instantly)
		{
			// Register the object for equipping
			m_WaitingToWield = true;
			m_NextTimeCanWield = Time.time;

			// Register the current equipped object for disabling
			if (m_EquipmentHandlers[0].CurrentItem != null)
			{
				if (Player.Reload.Active)
					Player.Reload.ForceStop();

				if (m_EquipmentHandlers[0].UsingItem.Active)
				{
					m_EquipmentHandlers[0].UsingItem.ForceStop();
					m_EquipmentHandlers[0].CurrentItem.OnUseEnd();
				}

				m_EquipmentHandlers[0].CurrentItem.Unwield();

				if (!instantly)
				{
					m_NextTimeCanWield += m_EquipmentHandlers[0].CurrentItem.UnWieldtime;
				}
			}

			Player.EquippedItem.Set(item);
		}

		private bool Try_DestroyHeldItem(float delay)
		{
			if (Player.EquippedItem.Get() == null)
				return false;
			else
			{
				if (delay <= 0f)
				{
					Player.Inventory.RemoveItem(Player.EquippedItem.Get());
					Player.EquippedItem.Set(null);
				}

				return true;
			}
		}

		private bool Try_SwapItems(SaveableItem item)
		{
			SaveableItem currentItem = Player.EquippedItem.Get();

			if (currentItem != null && IsSwappable(currentItem) && IsSwappable(item))
			{
				ItemSlot itemSlot = Player.Inventory.GetItemSlot(currentItem);

				if (Player.Inventory.TryDropItem(currentItem, true))
				{
					Player.DestroyEquippedItem.Try(0f);
					Player.EquipItem.Try(item, true);

					itemSlot.SetItem(item);

					return true;
				}
			}

			return false;
		}

		private bool IsSwappable(SaveableItem item)
		{
			if (item == null || item.Data == null)
			{
				Debug.LogError("Item instance or it's data is null!!");
				return false;
			}

			if (m_EquipableItemsCategories.ContainsKey(item.Data.Category))
				return true;

			return false;
		}

		private void Wield()
		{
			var item = Player.EquippedItem.Get();

			// Set the Currently equipped item active
			m_EquipmentHandlers[0].WieldItem(item);

			Player.ActiveEquipmentItem.Set(m_EquipmentHandlers[0].CurrentItem);
			m_ViewCamera.fieldOfView = m_EquipmentHandlers[0].CurrentItem.FieldOfView;
			m_WorldCamera.AimHeadBob = m_EquipmentHandlers[0].CurrentItem.AimCamHeadbob;
		}

		private void OnChanged_IsCloseToAnObject(bool isClose)
		{
			if (m_EquipmentHandlers[0].CurrentItem != null && Player.ObjectInProximity.Get() && Player.Aim.Active)
				Player.Aim.ForceStop();
		}

		private bool TryStart_Aim()
		{
			var item = m_EquipmentHandlers[0].CurrentItem;

			bool canStartAiming =
				!Player.Aim.Active &&
				item != null && item.EnableAiming && item.CanAim() && Time.time > m_NextTimeCanWield + item.WieldTime &&
				!Player.Pause.Active &&
				!Player.Run.Active &&
				!Player.Reload.Active;

			if (canStartAiming)
			{
				if (m_FOVSetter != null)
					StopCoroutine(m_FOVSetter);

				m_FOVSetter = StartCoroutine(C_SetFOV(m_AimFOV));

				SetPlayerMovementSpeed(m_AimPlayerSpeedMultiplier);

				if(PostProcessingManager.Instance != null)
					PostProcessingManager.Instance.EnableAimBlur(item.UseAimBlur);

				item.OnAimStart();
			}

			return canStartAiming;
		}

		private void OnStop_Aim()
		{
			if (m_FOVSetter != null)
				StopCoroutine(m_FOVSetter);

			if (m_WorldCamera.UnityCamera.fieldOfView != m_NormalFOV)
				m_FOVSetter = StartCoroutine(C_SetFOV(m_NormalFOV));

			PostProcessingManager.Instance.EnableAimBlur(false);

			SetPlayerMovementSpeed(1f);

			if (m_EquipmentHandlers[0].CurrentItem != null)
				m_EquipmentHandlers[0].CurrentItem.OnAimEnd();
		}

		private bool TryStart_Reload()
		{
			if (m_EquipmentHandlers[0].CurrentItem == null && Player.ActiveEquipmentItem.Val == null || (Player.Run.Active && !m_ReloadWhileRunning))
				return false;

			bool startedReloading = m_EquipmentHandlers[0].CurrentItem.TryStartReloading();

			if (Player.Aim.Active && startedReloading && !m_AimWhileReloading)
				Player.Aim.ForceStop();

			return startedReloading;
		}

		private void OnStop_Reload()
		{
			if (m_EquipmentHandlers[0].CurrentItem != null)
				m_EquipmentHandlers[0].CurrentItem.OnReloadEnd();
		}

		private bool Try_Use(bool continuously)
		{
			if (Player.Pause.Active || Player.Run.Active || m_WaitingToWield)
				return false;

			var item = m_EquipmentHandlers[0].CurrentItem;

			if (item != null)
			{
				if (item.CurrentAmmoInfo.Val.CurrentInMagazine == 0 && m_ReloadIfDryFire && item.CurrentAmmoInfo.Val.CurrentInStorage > 0)
				{
					Player.Reload.TryStart();
					return false;
				}

				bool airborneCondition = Player.IsGrounded.Get() || item.UseWhileAirborne;

				bool canTryToAttack =
					airborneCondition &&
					(item.StaminaTakePerUse == 0f || Player.Stamina.Get() > item.StaminaTakePerUse);

				if (!item.CanStopReloading && Player.Reload.Active)
					canTryToAttack = false;

				if (canTryToAttack && Time.time > m_EquipmentHandlers[0].LastChangeItemTime + item.WieldTime)
				{
					bool usedSuccessfully;

					if (continuously)
						usedSuccessfully = m_EquipmentHandlers[0].TryUseContinuously(m_WorldCamera.UnityCamera);
					else
					{
						if (Player.Reload.Active && item.CanStopReloading && item.CurrentAmmoInfo.Val.CurrentInMagazine > 0)
							Player.Reload.ForceStop();

						usedSuccessfully = m_EquipmentHandlers[0].TryUseOnce(m_WorldCamera.UnityCamera);
					}

					if (usedSuccessfully && item.StaminaTakePerUse > 0f)
						Player.Stamina.Set(Mathf.Clamp(Player.Stamina.Get() - item.StaminaTakePerUse, 0f, Mathf.Infinity));

					return usedSuccessfully;
				}
			}

			return false;
		}

		private void InitiateEquippableItems()
		{
			Wield();

			//Initiate the equippable item categories
			for (int n = 0; n < m_EquipableCategories.Length; n++)
			{
				m_EquipableItemsCategories.Add(m_EquipableCategories[n].Category, m_EquipableCategories[n].MovementSpeedMultiplier);
			}
		}

		private IEnumerator C_SetFOV(float targetFOV)
		{
			while (Mathf.Abs(m_WorldCamera.UnityCamera.fieldOfView - targetFOV) > Mathf.Epsilon)
			{
				m_WorldCamera.UnityCamera.fieldOfView = Mathf.MoveTowards(m_WorldCamera.UnityCamera.fieldOfView, targetFOV, Time.deltaTime * m_FOVSetSpeed);
				yield return null;
			}
		}

		private void SetPlayerMovementSpeed(float multiplier) 
		{
			if (m_EquipmentHandlers[0].CurrentlyAttachedItem != null)
				Player.MovementSpeedFactor.Set(1f * multiplier);
			else if (m_EquipableItemsCategories.TryGetValue(m_EquipmentHandlers[0].CurrentlyAttachedItem.Data.Category, out float playerSpeedMultiplier))
				Player.MovementSpeedFactor.Set(playerSpeedMultiplier * multiplier);			
		}
	}
}