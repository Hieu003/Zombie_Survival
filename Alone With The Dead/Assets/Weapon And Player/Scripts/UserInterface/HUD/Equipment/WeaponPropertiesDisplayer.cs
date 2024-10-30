using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HQFPSWeapons.UserInterface
{
	public class WeaponPropertiesDisplayer : HUD_DisplayerBehaviour
	{
		#region Internal
		[Serializable]
		public struct FireModeDisplayer
		{
			[BHeader("GENERAL", true)]

			public Image FireModeImage;

			[Space]

			[BHeader("Fire Mode Sprites...", order = 100)]

			public Sprite SafetyModeSprite;
			public Sprite SemiAutoModeSprite;
			public Sprite FullAutoModeSprite;
			public Sprite BurstModeSprite;
		}

		[Serializable]
		public class AmmoAmountDisplayer
		{
			[BHeader("GENERAL", true)]

			public Text StorageTxt;

			[Range(1, 100)]
			public int MaxMagSize = 30;

			[Range(1, 100)]
			[Tooltip("At what percent the ammo in the magazine is considered low (e.g. reload message will appear)")]
			public float LowAmmoPercent = 30;

			[Space]

			public GridLayoutGroup BulletsLayoutGroup;
			public Image BulletImage;

			[Space]

			public Color NormalBulletColor = Color.white;
			public Color LowAmmoBulletColor = Color.red;
			public Color BulletConsumedColor = Color.black;

			[Space]

			[Group]
			public BulletDisplayer[] BulletDisplayers;

			[Space]

			[BHeader("Reload Message...", order = 100)]

			public Image ReloadMessage = null;

			// Not visible in the inspector
			public List<Image> BulletImages = new List<Image>(30);

			// -- Internal -- 
			[Serializable]
			public struct BulletDisplayer
			{
				[DatabaseItem]
				public string CorrespondingItem;

				public Sprite BulletSprite;

				public Vector2 BulletSpriteSize;

				public Vector2 LayoutGroupSpacing;

				public int XOffset;

				public float BulletLineWidth;
			}
		}
		#endregion

		[SerializeField]
		private Animator m_Animator = null;

		[SerializeField]
		private Image m_WeaponIconImg = null;

		[Space]

		[SerializeField]
		[Group]
		private AmmoAmountDisplayer m_AmmoDisplayer = null;

		[SerializeField]
		[Group]
		private FireModeDisplayer m_FireModeDisplayer = new FireModeDisplayer();

		private int m_LastCountInMagazine;
		private bool m_IsNewWeapon;

		private EquipmentHandler m_MainEquipmentHandler;
		private RectTransform m_BulletsLayoutGroupRct;

		
		public override void OnPostAttachment()
		{
			OnActiveEquipmentItemChanged(null);
			Player.ActiveEquipmentItem.AddChangeListener(OnActiveEquipmentItemChanged);
			Player.ChangeFireMode.AddListener(UpdateFireModeUI);

			m_MainEquipmentHandler = Player.transform.GetComponentInChildren<EquipmentHandler>();

			//Spawn the bullet images and remove the original one
			if (m_AmmoDisplayer.BulletImage != null)
			{
				for (int i = 0; i < m_AmmoDisplayer.MaxMagSize; i++)
					m_AmmoDisplayer.BulletImages.Add(Instantiate(m_AmmoDisplayer.BulletImage,
						m_AmmoDisplayer.BulletImage.rectTransform.position, //Position
						m_AmmoDisplayer.BulletImage.rectTransform.rotation, //Rotation
						m_AmmoDisplayer.BulletsLayoutGroup.transform));     //Parent

				m_AmmoDisplayer.BulletImage.enabled = false;

				m_BulletsLayoutGroupRct = m_AmmoDisplayer.BulletsLayoutGroup.GetComponent<RectTransform>();
			}
		}

		private void OnActiveEquipmentItemChanged(EquipmentItem Weapon)
		{
			if (m_MainEquipmentHandler == null)
				return;

			if (m_MainEquipmentHandler.CurrentItem != null)
				m_MainEquipmentHandler.CurrentItem.CurrentAmmoInfo.RemoveChangeListener(UpdateAmmoUI);

			m_IsNewWeapon = true;

			if (m_MainEquipmentHandler.CurrentItem == null || !m_MainEquipmentHandler.CurrentItem.NeedsAmmoToUse)
			{
				m_Animator.SetBool("Show", false);
				return;
			}
			else
				m_Animator.SetBool("Show", true);

			m_LastCountInMagazine = m_MainEquipmentHandler.CurrentItem.CurrentAmmoInfo.Get().CurrentInMagazine;

			m_WeaponIconImg.sprite = m_MainEquipmentHandler.CurrentlyAttachedItem.Data.Icon;

			UpdateAmmoUI(m_MainEquipmentHandler.CurrentItem.CurrentAmmoInfo.Get());
			m_MainEquipmentHandler.CurrentItem.CurrentAmmoInfo.AddChangeListener(UpdateAmmoUI);
			
			//Change the bullet images
			if (m_MainEquipmentHandler.CurrentItem != null)
			{
				for (int i = 0; i < m_AmmoDisplayer.BulletDisplayers.Length; i++)
				{
					if (m_AmmoDisplayer.BulletDisplayers[i].CorrespondingItem == m_MainEquipmentHandler.CurrentlyAttachedItem.Data.Name)
					{
						for (int j = 0; j < m_AmmoDisplayer.BulletImages.Count; j++)
						{
							m_AmmoDisplayer.BulletImages[j].gameObject.SetActive(false);
						}

						for (int j = 0; j < m_MainEquipmentHandler.CurrentItem.TryGetMagazineSize(); j++)
						{
							m_AmmoDisplayer.BulletImages[j].gameObject.SetActive(true);
							m_AmmoDisplayer.BulletImages[j].transform.localScale = m_AmmoDisplayer.BulletDisplayers[i].BulletSpriteSize;
							m_AmmoDisplayer.BulletImages[j].sprite = m_AmmoDisplayer.BulletDisplayers[i].BulletSprite;
						}

						m_BulletsLayoutGroupRct.sizeDelta = new Vector2(m_AmmoDisplayer.BulletDisplayers[i].BulletLineWidth, m_BulletsLayoutGroupRct.sizeDelta.y);
						m_AmmoDisplayer.BulletsLayoutGroup.spacing = m_AmmoDisplayer.BulletDisplayers[i].LayoutGroupSpacing;
						m_AmmoDisplayer.BulletsLayoutGroup.padding.left = m_AmmoDisplayer.BulletDisplayers[i].XOffset;
					}
				}
			}

			// Disable the firing mode image if the currently equipped weapon doesn't have a fire mode property
			if (m_MainEquipmentHandler.CurrentlyAttachedItem.HasProperty("FireMode"))
				m_FireModeDisplayer.FireModeImage.enabled = true;
			else
				m_FireModeDisplayer.FireModeImage.enabled = false;

			UpdateFireModeUI();

			m_IsNewWeapon = false;
		}

		private void UpdateAmmoUI(EquipmentItem.AmmoInfo ammoInfo)
		{
			if(m_MainEquipmentHandler == null)
				return;

			int newCountInMagazine = m_MainEquipmentHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine;

			//If the player used the Weapon, consume the bullet
			if (!m_IsNewWeapon && m_Animator != null && m_LastCountInMagazine > newCountInMagazine)
			{
				m_Animator.SetTrigger("Ammo Consumed");
				m_AmmoDisplayer.BulletImages[m_MainEquipmentHandler.CurrentItem.TryGetMagazineSize() - ammoInfo.CurrentInMagazine - 1].color = m_AmmoDisplayer.BulletConsumedColor;
			}
			//If the player is reloading add the bullets back up
			else if (m_LastCountInMagazine < newCountInMagazine && !m_IsNewWeapon)
			{
				for (int i = m_MainEquipmentHandler.CurrentItem.TryGetMagazineSize() - ammoInfo.CurrentInMagazine; i < m_AmmoDisplayer.BulletImages.Count; i++)
				{
					m_AmmoDisplayer.BulletImages[i].color = m_AmmoDisplayer.NormalBulletColor;
				}
			}
			//If the player changed weapons activate as many bullets as the current weapon has in the magazine
			else if (m_IsNewWeapon)
			{
				for (int i = 0; i < m_MainEquipmentHandler.CurrentItem.TryGetMagazineSize(); i++)
				{
					m_AmmoDisplayer.BulletImages[i].color = m_AmmoDisplayer.BulletConsumedColor;
				}

				for (int i = m_MainEquipmentHandler.CurrentItem.TryGetMagazineSize() - newCountInMagazine; i < m_MainEquipmentHandler.CurrentItem.TryGetMagazineSize(); i++)
				{
					m_AmmoDisplayer.BulletImages[i].color = m_AmmoDisplayer.NormalBulletColor;
				}
			}

			//Activate Reload Message.
			if (newCountInMagazine <= m_MainEquipmentHandler.CurrentItem.TryGetMagazineSize() * (m_AmmoDisplayer.LowAmmoPercent / 100))
			{
				m_AmmoDisplayer.ReloadMessage.gameObject.SetActive(true);
			}
			else
				m_AmmoDisplayer.ReloadMessage.gameObject.SetActive(false);

			//Change the bullets color to the "low bullet color".
			if (newCountInMagazine <= m_MainEquipmentHandler.CurrentItem.TryGetMagazineSize() * (m_AmmoDisplayer.LowAmmoPercent / 100))
			{
				for (int i = 0; i < m_AmmoDisplayer.BulletImages.Count; i++)
				{
					if (m_AmmoDisplayer.BulletImages[i].color == m_AmmoDisplayer.NormalBulletColor)
						m_AmmoDisplayer.BulletImages[i].color = m_AmmoDisplayer.LowAmmoBulletColor;
				}
			}
			//Change the bullets color back to normal bullet color.
			else
			{
				for (int i = 0; i < m_AmmoDisplayer.BulletImages.Count; i++)
				{
					if(m_AmmoDisplayer.BulletImages[i].color == m_AmmoDisplayer.LowAmmoBulletColor)
						m_AmmoDisplayer.BulletImages[i].color = m_AmmoDisplayer.NormalBulletColor;
				}
			}

			//Update the storage Text
			m_AmmoDisplayer.StorageTxt.text = m_MainEquipmentHandler.CurrentItem.CurrentAmmoInfo.Get().CurrentInStorage.ToString();

			m_LastCountInMagazine = newCountInMagazine;
		}

		private void UpdateFireModeUI() 
		{
			//If the current weapon doesn't have a "Fire Mode" disable the fire mode image
			if (m_MainEquipmentHandler.CurrentItem == null || !m_MainEquipmentHandler.CurrentlyAttachedItem.HasProperty("FireMode"))
			{
				m_FireModeDisplayer.FireModeImage.color = Color.clear;
				return;
			}
			else
				m_FireModeDisplayer.FireModeImage.color = Color.white;

			//Get fire mode
			m_Animator.SetTrigger("Fire Mode Changed");

			int fireMode = (int)m_MainEquipmentHandler.CurrentlyAttachedItem.GetProperty("FireMode").Val.Current;

			//Get the fire mode property
			if (fireMode == (int)ProjectileBasedWeapon.FireMode.Burst)
				m_FireModeDisplayer.FireModeImage.sprite = m_FireModeDisplayer.BurstModeSprite;
			else if (fireMode == (int)ProjectileBasedWeapon.FireMode.FullAuto)
				m_FireModeDisplayer.FireModeImage.sprite = m_FireModeDisplayer.FullAutoModeSprite;
			else if (fireMode == (int)ProjectileBasedWeapon.FireMode.SemiAuto)
				m_FireModeDisplayer.FireModeImage.sprite = m_FireModeDisplayer.SemiAutoModeSprite;
			else if (fireMode == (int)ProjectileBasedWeapon.FireMode.Safety)
				m_FireModeDisplayer.FireModeImage.sprite = m_FireModeDisplayer.SafetyModeSprite;
		}
    }
}
