using System;
using System.Collections;
using UnityEngine;

namespace HQFPSWeapons
{
	public class RevolverCyllinderCorrector : EquipmentCorrector
	{
		#region Internal
		[Serializable]
		private class CyllinderCorrector
		{
			[BHeader("General", true)]

			[HideInInspector]
			public Transform Cyllinder = null;

			public string CyllinderBoneName = "";

			[Space]

			public Vector3 RotationPerShot = Vector3.zero;

			[SerializeField]
			[Range(0f, 10f)]
			public float RotationDelay = 0.5f;

			[SerializeField]
			[Range(0f, 25f)]
			public float RotationSpeed = 1f;

			[Space]

			[SerializeField]
			public float ReloadResetCyllinderDelay = 0f;
		}
		#endregion

		[SerializeField]
		[Group]
		private CyllinderCorrector m_CyllinderCorrector = null;

		private Vector3 m_CyllinderRot;
		private Vector3 m_NewCyllinderRot;

		private int m_LastAmmoAmount;

		WaitForSeconds m_RotationWait;


		protected override void Start()
		{
			base.Start();

			m_CyllinderCorrector.Cyllinder = m_EHandler.EquipSettings.Armature.FindDeepChild(m_CyllinderCorrector.CyllinderBoneName);

			m_RotationWait = new WaitForSeconds(m_CyllinderCorrector.RotationDelay);
			m_LastAmmoAmount = m_EHandler.CurrentItem.CurrentAmmoInfo.Val.CurrentInMagazine;
		}

		protected override void OnReloadStart()
		{
			base.OnReloadStart();

			StartCoroutine(C_ReloadResetCyllinder());
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();

			if (Player.ActiveEquipmentItem.Is(m_EHandler.CurrentItem))
			{
				Quaternion newRotation = Quaternion.Euler(m_CyllinderCorrector.Cyllinder.localEulerAngles + m_CyllinderRot);

				m_CyllinderCorrector.Cyllinder.localRotation = newRotation;
			}
		}

		protected override void OnAmmoChanged(EquipmentItem.AmmoInfo ammoInfo)
		{
			base.OnAmmoChanged(ammoInfo);

			if (Player.Reload.Active && !Player.ActiveEquipmentItem.Is(m_EHandler.CurrentItem))
				return;

			if (ammoInfo.CurrentInMagazine < m_LastAmmoAmount)
				StartCoroutine(C_DelayedRotation());

			m_LastAmmoAmount = ammoInfo.CurrentInMagazine;
		}

		IEnumerator C_ReloadResetCyllinder()
		{
			yield return new WaitForSeconds(m_CyllinderCorrector.ReloadResetCyllinderDelay);

			m_CyllinderRot = Vector3.zero;
			m_NewCyllinderRot = Vector3.zero;
		}

		IEnumerator C_DelayedRotation()
		{
			yield return m_RotationWait;

			WaitForEndOfFrame wait = new WaitForEndOfFrame();

			m_NewCyllinderRot = m_CyllinderRot + m_CyllinderCorrector.RotationPerShot;

			while (m_CyllinderRot != m_NewCyllinderRot)
			{
				m_CyllinderRot = Vector3.Lerp(m_CyllinderRot, m_NewCyllinderRot, m_CyllinderCorrector.RotationSpeed * Time.deltaTime);

				yield return wait;
			}
		}
    }
}