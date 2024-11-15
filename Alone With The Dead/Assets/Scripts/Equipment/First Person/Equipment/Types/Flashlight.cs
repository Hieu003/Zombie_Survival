using System;
using System.Collections;
using UnityEngine;

namespace HQFPSWeapons
{
	public class Flashlight : EquipmentItem
	{
		[BHeader("FLASHLIGHT", true)]

		[SerializeField]
		[Group]
		private FlashlightSettings m_FlashlightSettings = null;

		[SerializeField]
		[Group]
		private FlashlightAudioSettings m_FlashlightAudio = null;

		private bool m_LastSwitchState;

		private WaitForSeconds SwitchDuration;


		public override bool TryUseOnce(Camera camera)
		{
			if(m_NextTimeCanUse > Time.time)
				return false;

			m_NextTimeCanUse = Time.time + m_UseThreshold;

			m_EHandler.PlayDelayedSound(m_LastSwitchState == true ? m_FlashlightAudio.SwitchOffClip : m_FlashlightAudio.SwitchOnClip);
			m_EHandler.Animator.SetTrigger("Use");

			m_LastSwitchState = !m_LastSwitchState;

			if(m_FlashlightSettings.Light != null)
				StartCoroutine(C_EnableLight());

			return true;
		}

        private void OnValidate()
        {
			if (m_EHandler != null)
				Start();
		}

        private void Start()
		{
			m_EHandler.Animator.SetFloat("Use Speed", m_FlashlightSettings.AnimSwitchSpeed);
			SwitchDuration = new WaitForSeconds(m_FlashlightSettings.SwitchDuration);
			m_UseThreshold = m_FlashlightSettings.SwitchDuration;
		}

		private IEnumerator C_EnableLight()
		{
			yield return SwitchDuration;

			if (m_LastSwitchState)
				m_FlashlightSettings.Light.Play(m_FlashlightSettings.LightFadeIn);
			else
				m_FlashlightSettings.Light.Stop(m_FlashlightSettings.LightFadeIn);
		}

        #region Internal
        [Serializable]
		public class FlashlightSettings
		{
			public LightEffect Light;

			public bool LightFadeIn;

			[Range(0.1f, 2f)]
			public float SwitchDuration = 0.5f;

			[Range(0.1f, 3f)]
			public float AnimSwitchSpeed = 1f;
		}

		[Serializable]
		public class FlashlightAudioSettings
		{
			public DelayedSound SwitchOnClip;
			public DelayedSound SwitchOffClip;
		}
        #endregion
    }
}