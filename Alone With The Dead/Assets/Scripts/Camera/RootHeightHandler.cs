using System.Collections;
using UnityEngine;

namespace HQFPSWeapons
{
	public class RootHeightHandler : PlayerComponent  
	{
		[SerializeField]
		[Clamp(-2f, 0f)]
		private float m_CrouchOffset = -1f;

		[SerializeField]
		private EasingOptions m_CrouchEasing = new EasingOptions();

		private float m_CurrentOffsetOnY;
		private float m_InitialHeight;

		private Easer m_HeightEaser;


		private void Start()
		{
			Player.Crouch.AddStartListener(OnCrouchStart);
			Player.Crouch.AddStopListener(OnCrouchStop);
			m_InitialHeight = transform.localPosition.y;

			m_HeightEaser = new Easer(m_CrouchEasing.Function, m_CrouchEasing.Duration);
		}

		private void OnCrouchStart()
		{
			StopAllCoroutines();
			StartCoroutine(SetVerticalOffset(m_CrouchOffset));
		}

		private void OnCrouchStop()
		{
			StopAllCoroutines();
			StartCoroutine(SetVerticalOffset(0f));
		}

		private IEnumerator SetVerticalOffset(float offset)
		{
			var startOffset = m_CurrentOffsetOnY;
			m_HeightEaser.Reset();

			while(m_HeightEaser.InterpolatedValue < 1f)
			{
				m_HeightEaser.Update(Time.deltaTime);
				m_CurrentOffsetOnY = Mathf.Lerp(startOffset, offset, m_HeightEaser.InterpolatedValue);

				transform.localPosition = Vector3.up * (m_CurrentOffsetOnY + m_InitialHeight);

				yield return null;
			}
		}
	}
}
