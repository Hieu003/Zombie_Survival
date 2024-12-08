using System;
using UnityEngine;
using UnityEngine.UI;

namespace HQFPSWeapons.UserInterface
{
	public class InteractionInterface : UserInterfaceBehaviour
	{
		[BHeader("GENERAL", true)]

		[SerializeField]
		private Animator m_MainAnimator = null;

		[SerializeField]
		private Animator m_SwapAnimator = null;

		[Space]

		[SerializeField]
		private Text m_MainText = null;

		[Space]

		[SerializeField]
		[Group]
		private WeaponSwappingModule m_WeaponSwapping = new WeaponSwappingModule();

		private RaycastData m_RaycastData;

		private ItemPickup m_ItemPickup;


		public override void OnPostAttachment()
		{
			Player.RaycastData.AddChangeListener(OnPlayerRaycastChanged);

			Player.EquippedItem.AddChangeListener(ChangeVisibilityOfSwapIcon);
		}

		private void OnPlayerRaycastChanged(RaycastData raycastData)
		{
			bool show = raycastData != null && raycastData.IsInteractive;
			m_RaycastData = raycastData;

			if (show)
			{
				m_MainAnimator.SetBool("Show", true);

				if (m_WeaponSwapping.SwapIcon != null)
				{
					bool enableSwapUI = false;
					m_MainText.text = m_RaycastData.InteractiveObject.InteractionText;

					// Current item
					SaveableItem currentItem = Player.EquippedItem.Get();

					if (currentItem != null && Player.ItemIsSwappable.Try(currentItem))
					{
						// Detected item
						ItemPickup pickup = m_RaycastData.InteractiveObject as ItemPickup;
						
						//Make sure the item is swappable and check if the inventory is full
						enableSwapUI = pickup != null && Player.ItemIsSwappable.Try(pickup.ItemInstance) &&
									   Player.Inventory.GetContainerWithFlags(pickup.TargetContainers).ContainerIsFull();

						if (enableSwapUI)
						{
							m_ItemPickup = pickup;

							m_ItemPickup.NeedToSwap = true;

							m_WeaponSwapping.EquippedItemImg.sprite = Player.EquippedItem.Val.Data.Icon;
							m_WeaponSwapping.GroundItemImg.sprite = m_ItemPickup.ItemInstance.Data.Icon;
						}
					}

					EnableSwapUI(enableSwapUI);
				}
			}
			else
			{
				m_MainAnimator.SetBool("Show", false);
				m_SwapAnimator.SetBool("Show", false);

				if (m_ItemPickup != null)
				{
					m_ItemPickup.NeedToSwap = false;
					m_ItemPickup = null;
				}
			}
		}

		private void ChangeVisibilityOfSwapIcon(SaveableItem item) 
		{
			bool enable = (item != null);

			if(enable)
				m_WeaponSwapping.EquippedItemImg.sprite = item.Data.Icon;

			if(Player.RaycastData != null)
				EnableSwapUI(enable);
		}

		private void Update()
		{
			if (m_ItemPickup != null)
				UpdateSwapFill(m_ItemPickup.InterractionProgress);
		}

		private void EnableSwapUI(bool enable)
		{
			if (Player.RaycastData.Val != null)
			{
				if (Player.RaycastData.Val.IsInteractive)
				{
					m_MainAnimator.SetBool("Show", !enable);
					m_SwapAnimator.SetBool("Show", enable);
				}
			}
			else
			{
				m_MainAnimator.SetBool("Show", false);
				m_SwapAnimator.SetBool("Show", false);
			}

			if (m_ItemPickup != null && enable == false)
				m_ItemPickup.NeedToSwap = false;
		}

		private void UpdateSwapFill(float amount) 
		{
			m_WeaponSwapping.SwapIcon.fillAmount = amount * (1 / m_ItemPickup.SwapTime);
		}

		#region Internal
		#pragma warning disable 0649

		[Serializable]
		private struct WeaponSwappingModule
		{
			public Image SwapIcon;

			public Image EquippedItemImg;

			public Image GroundItemImg;
		}
        #endregion
    }
}
