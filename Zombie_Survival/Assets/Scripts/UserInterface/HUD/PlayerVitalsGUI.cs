using UnityEngine;
using UnityEngine.UI;

namespace HQFPSWeapons.UserInterface
{
	public class PlayerVitalsGUI : HUD_DisplayerBehaviour
	{
		[SerializeField]
		private Image m_HealthBar = null;

		[SerializeField]
		private Image m_StaminaBar = null;


		public override void OnPostAttachment()
		{
			Player.Health.AddChangeListener(OnChanged_Health);
			Player.Stamina.AddChangeListener(OnChanged_Stamina);

			OnChanged_Health(Player.Health.Get());
			OnChanged_Stamina(Player.Stamina.Get());
		}

		private void OnChanged_Health(float health)
		{
			m_HealthBar.fillAmount = health / 100f;
		}

		private void OnChanged_Stamina(float stamina)
		{
			m_StaminaBar.fillAmount = stamina / 100f;
		}
	}
}
