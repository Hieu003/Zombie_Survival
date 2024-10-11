using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HQFPSWeapons.UserInterface
{
	public class ScreenFader : UserInterfaceBehaviour
	{
		[SerializeField]
		private Image m_Image = null;
		
		[SerializeField]
		private float m_FadeSpeed = 0.3f;

		[SerializeField]
		private float m_FadePause = 1f;

		private float m_CurrentFadeSpeed = 1f;


		public override void OnPostAttachment()
		{
			Player.Death.AddListener(On_Death);
			Player.Respawn.AddListener(On_Respawn);

			m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 1f);
			StartCoroutine(C_FadeScreen(0f, m_FadePause));
		}

		private void On_Death()
		{
			StopAllCoroutines();
			StartCoroutine(C_FadeScreen(1f));
		}

		private void On_Respawn()
		{
			StopAllCoroutines();
			StartCoroutine(C_FadeScreen(0f, m_FadePause));
		}

		private IEnumerator C_FadeScreen(float targetAlpha, float pause = 0f)
		{
			m_CurrentFadeSpeed = 1f;

			yield return new WaitForSeconds(pause);

			while (Mathf.Abs(m_Image.color.a - targetAlpha) > 0f)
			{
				m_Image.color = MoveTowardsAlpha(m_Image.color, targetAlpha, Time.deltaTime * m_FadeSpeed * m_CurrentFadeSpeed);

				m_CurrentFadeSpeed += Time.deltaTime;

				yield return null;
			}
		}

		private Color MoveTowardsAlpha(Color color, float alpha, float maxDelta)
		{
			return new Color(color.r, color.g, color.b, Mathf.MoveTowards(color.a, alpha, maxDelta));
		}
	}
}
