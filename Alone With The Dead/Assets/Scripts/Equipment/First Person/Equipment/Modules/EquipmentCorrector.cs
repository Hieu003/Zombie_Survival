using System;
using System.Collections;
using UnityEngine;

namespace HQFPSWeapons
{
	public class EquipmentCorrector : FPEquipmentComponent
	{
		#region Internal
		private enum CartridgeChangeMethod { DisableBulletMesh, DisableWholeCartridge, Mixed, ChangeCartridgeMesh }

		[Serializable]
		private class MovingPartsCorrector
		{
			[BHeader("GENERAL", true)]

			public float MoveToEmptyPosDelay = 0.1f;

			[Group]
			public MovingPart[] MovingParts = null;

			[Serializable]
			public class MovingPart
			{
				public string BoneName = "";

				public Vector3 EmptyPosition = Vector3.zero;
				public Vector3 EmptyRotation = Vector3.zero;

				[HideInInspector]
				public Transform MovingPartBone = null;

				[HideInInspector]
				public Vector3 OriginalPosition = Vector3.zero;

				[HideInInspector]
				public Vector3 OriginalRotation = Vector3.zero;
			}
		}

		[Serializable]
		private class CartridgeCorrector
		{
			[BHeader("GENERAL", true)]

			public CartridgeChangeMethod CartridgeChangeMethod = CartridgeChangeMethod.ChangeCartridgeMesh;

			public bool ChangeCartridgeWhileFiring = false;

			[ShowIf("ChangeCartridgeWhileFiring", true)]
			public float ChangeCartridgeDelay = 0f;

			public CartridgeObject CartridgePrefab = null;

			public bool FakeExtraCartridges = false;

			[Space(3f)]

			public Vector3[] BulletOffsets = null;

			public Vector3 BulletsRotation = Vector3.zero;

			[Space(5f)]

			[BHeader("Delays", order = 100)]

			public float CassingDelay = 0.5f;

			public float CartridgeDelay = 1f;

			public float DisableAllCartridgesDelay = 0.6f;
		}
		#endregion

		[SerializeField]
		[Group]
		private CartridgeCorrector m_CartridgeCorrector = null;

		[SerializeField]
		[Group]
		private MovingPartsCorrector m_MovingPartsCorrector = null;

		private CartridgeObject[] m_Cartridges;
		private bool m_WeaponIsEmpty;


        protected virtual void OnAmmoChanged(EquipmentItem.AmmoInfo ammoInfo) 
		{
			if (m_CartridgeCorrector.ChangeCartridgeWhileFiring && 
				(ammoInfo.CurrentInMagazine != m_EHandler.CurrentItem.CurrentAmmoInfo.PrevVal.CurrentInMagazine ||
				ammoInfo.CurrentInMagazine != m_EHandler.CurrentItem.TryGetMagazineSize()))
			{
				StartCoroutine(C_ShootEmpty());

				if (m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine <= 0)
					StartCoroutine(C_StartTheMovingParts());
			}
		}

		protected virtual void OnReloadStart()
		{
			m_WeaponIsEmpty = false;

			if (Player.ActiveEquipmentItem.Is(m_EHandler.CurrentItem))
			{
				if (m_Cartridges != null && m_CartridgeCorrector.CartridgePrefab != null)
					StartCoroutine(C_ChangeCartridges());
			}
		}

		protected virtual void LateUpdate() 
		{
			if (Player.ActiveEquipmentItem.Val != m_EHandler.CurrentItem)
				return;

			if (Player.ActiveEquipmentItem.Is(m_EHandler.CurrentItem) && m_WeaponIsEmpty)
			{
				for (int i = 0; i < m_MovingPartsCorrector.MovingParts.Length; i++)
				{
					if (m_MovingPartsCorrector.MovingParts[i].EmptyPosition != Vector3.zero)
					{
						Vector3 newPosition = new Vector3(
								m_MovingPartsCorrector.MovingParts[i].MovingPartBone.localPosition.x + m_MovingPartsCorrector.MovingParts[i].EmptyPosition.x,
								m_MovingPartsCorrector.MovingParts[i].MovingPartBone.localPosition.y + m_MovingPartsCorrector.MovingParts[i].EmptyPosition.y,
								m_MovingPartsCorrector.MovingParts[i].MovingPartBone.localPosition.z + m_MovingPartsCorrector.MovingParts[i].EmptyPosition.z);

						m_MovingPartsCorrector.MovingParts[i].MovingPartBone.localPosition = newPosition;
					}

					if (m_MovingPartsCorrector.MovingParts[i].EmptyRotation != Vector3.zero)
					{
						Vector3 originalEulerAngles = m_MovingPartsCorrector.MovingParts[i].MovingPartBone.localEulerAngles;

						Vector3 newEulerAngles = new Vector3(
								originalEulerAngles.x + m_MovingPartsCorrector.MovingParts[i].EmptyRotation.x,
								originalEulerAngles.y + m_MovingPartsCorrector.MovingParts[i].EmptyRotation.y,
								originalEulerAngles.z + m_MovingPartsCorrector.MovingParts[i].EmptyRotation.z);

						m_MovingPartsCorrector.MovingParts[i].MovingPartBone.localEulerAngles = newEulerAngles;
					}
				}
			}
		}

		protected virtual void Start()
		{
			for (int i = 0; i < m_MovingPartsCorrector.MovingParts.Length; i++)
			{
				m_MovingPartsCorrector.MovingParts[i].MovingPartBone = m_EHandler.EquipSettings.Armature.FindDeepChild(m_MovingPartsCorrector.MovingParts[i].BoneName);
			}

			m_EHandler.CurrentItem.CurrentAmmoInfo.AddChangeListener(OnAmmoChanged);
			Player.Reload.AddStartListener(OnReloadStart);

			OnAmmoChanged(m_EHandler.CurrentItem.CurrentAmmoInfo.Val);

			if (m_CartridgeCorrector.CartridgePrefab != null)
			{
				m_Cartridges = new CartridgeObject[m_CartridgeCorrector.BulletOffsets.Length];

				for (int i = 0; i < m_Cartridges.Length; i++)
				{
					m_Cartridges[i] = Instantiate(m_CartridgeCorrector.CartridgePrefab, m_EHandler.EquipSettings.BulletBones[i]);
					m_Cartridges[i].transform.localPosition += m_CartridgeCorrector.BulletOffsets[i];
					m_Cartridges[i].transform.localEulerAngles += m_CartridgeCorrector.BulletsRotation;
				}
			}
		}

		private void OnDestroy()
		{
			Player.Reload.RemoveStartListener(OnReloadStart);
			m_EHandler.CurrentItem.CurrentAmmoInfo.RemoveChangeListener(OnAmmoChanged);

			if (m_Cartridges != null)
			{
				foreach (var cartridge in m_Cartridges)
				{
					Destroy(cartridge.gameObject);
				}
			}
		}

		private IEnumerator C_StartTheMovingParts() 
		{
			yield return new WaitForSeconds(m_MovingPartsCorrector.MoveToEmptyPosDelay);

			if (!Player.Reload.Active)
				m_WeaponIsEmpty = true;
		}

		private IEnumerator C_ShootEmpty() 
		{
			yield return new WaitForSeconds(m_CartridgeCorrector.ChangeCartridgeDelay);

			if ((m_CartridgeCorrector.FakeExtraCartridges && m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine > 0) || m_CartridgeCorrector.CartridgePrefab == null)
				yield break;

			int CartridgeNumber = m_EHandler.CurrentItem.TryGetMagazineSize() - m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine;

			if (m_CartridgeCorrector.FakeExtraCartridges)
				CartridgeNumber = 1;

			if (CartridgeNumber == 0)
				yield break;

			if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.DisableBulletMesh)
				m_Cartridges[CartridgeNumber - 1].ChangeBulletState(false);
			else if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.DisableWholeCartridge)
				m_Cartridges[CartridgeNumber - 1].ChangeCartridgeState(false);
			else if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.Mixed)
				m_Cartridges[CartridgeNumber - 1].ChangeBulletState(false);
			else if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.ChangeCartridgeMesh)
				m_Cartridges[CartridgeNumber - 1].ChangeCartridgeMesh(false);
		}

		private IEnumerator C_ChangeCartridges()
		{
			//Usefull for weapons that use magazines
			if (m_CartridgeCorrector.FakeExtraCartridges)
			{
				if (m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine == 0)
				{
					yield return new WaitForSeconds(m_CartridgeCorrector.CartridgeDelay);
					m_Cartridges[0].ChangeCartridgeState(true);
				}

				yield break;
			}

			int numberOfEmptyCartridges = m_EHandler.CurrentItem.TryGetMagazineSize() - m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine;
			bool enoughCartridges = true;

			//Enabled only if CartridgeChangeMethod is "Mixed" or "Change Cartridge Mesh"
			if ((m_EHandler.CurrentItem.TryGetMagazineSize() > m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInStorage || m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine > m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInStorage)
				&& (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.Mixed))
			{
				yield return new WaitForSeconds(m_CartridgeCorrector.DisableAllCartridgesDelay);

				numberOfEmptyCartridges = m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInStorage;
				enoughCartridges = false;

				for (int i = 0; i < m_EHandler.CurrentItem.TryGetMagazineSize() - m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine; i++)
				{
					m_Cartridges[i].ChangeCartridgeState(false);
					m_Cartridges[i].ChangeBulletState(false);
				}
			}
			else if(m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.Mixed)
				numberOfEmptyCartridges = m_EHandler.CurrentItem.TryGetMagazineSize();

			//Active only if Change Cartridge While Firing is disabled
			if (!m_CartridgeCorrector.ChangeCartridgeWhileFiring && enoughCartridges)
			{
				yield return new WaitForSeconds(m_CartridgeCorrector.CassingDelay);

				for (int i = 0; i < numberOfEmptyCartridges; i++)
				{
					if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.DisableBulletMesh)
						m_Cartridges[i].ChangeBulletState(false);
					else if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.DisableWholeCartridge)
						m_Cartridges[i].ChangeCartridgeState(false);
				}
			}

			float cartridgeDelay = enoughCartridges ? m_CartridgeCorrector.CartridgeDelay : m_CartridgeCorrector.CartridgeDelay - m_CartridgeCorrector.DisableAllCartridgesDelay;

			yield return new WaitForSeconds(cartridgeDelay);

			for (int i = 0; i < numberOfEmptyCartridges; i++)
			{
				if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.ChangeCartridgeMesh)
				{
					m_Cartridges[i].ChangeCartridgeMesh(true);
				}
				else
				{
					if (enoughCartridges)
					{
						if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.DisableBulletMesh)
							m_Cartridges[i].ChangeBulletState(true);
						else if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.DisableWholeCartridge)
							m_Cartridges[i].ChangeCartridgeState(true);
						else if (m_CartridgeCorrector.CartridgeChangeMethod == CartridgeChangeMethod.Mixed)
						{
							m_Cartridges[i].ChangeBulletState(true);
							m_Cartridges[i].ChangeCartridgeState(true);
						}
					}
					else
					{
						m_Cartridges[i].ChangeBulletState(true);
						m_Cartridges[i].ChangeCartridgeState(true);
					}
				}
			}
		}
	}
}