using UnityEngine.UI;
using UnityEngine;

namespace HQFPSWeapons.UserInterface
{
    public class HealsDisplayer : HUD_DisplayerBehaviour
    {
        [BHeader("Heals HUD...")]

        [SerializeField]
        private Text m_HealsAmountText = null;

        [SerializeField]
        private string m_HealsContainerName = "Backpack";

        [BHeader("Heal! (Low Health Message...)")]

        [SerializeField]
        private float m_LowHealthThreshold = 0f;

        [SerializeField]
        private CanvasGroup m_HealCanvas = null;


        public override void OnPostAttachment()
        {
            OnInventoryChanged();

            Player.Inventory.ContainerChanged.AddListener(OnInventoryChanged);
            Player.Healing.AddStopListener(OnEndHealing);
            Player.Health.AddChangeListener(OnPlayerChangeHealth);

            OnPlayerChangeHealth(Player.Health.Val);
        }

        private void OnEndHealing()
        {
            OnInventoryChanged();
        }

        private void OnPlayerChangeHealth(float healthAmount) 
        {
            if (Player.Health.Val == 0)
            {
                m_HealCanvas.alpha = 0;
                return;
            }

            if (healthAmount < m_LowHealthThreshold)
                m_HealCanvas.alpha = 1;
            else if (m_HealCanvas.gameObject.activeSelf)
                m_HealCanvas.alpha = 0;
        }

        private void OnInventoryChanged()
        {
            int healsAmount = 0;

            ItemContainer container = Player.Inventory.GetContainerWithName(m_HealsContainerName);

            if (container != null)
            {
                foreach (var slot in container.Slots)
                {
                    if (slot.HasItem)
                        healsAmount++;
                }

                m_HealsAmountText.text = "x " + healsAmount.ToString();
            }
            else
            {
                m_HealsAmountText.text = "NULL";
            }
        }
    }
}
