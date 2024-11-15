using UnityEngine;
using UnityEngine.UI;

namespace HQFPSWeapons.UserInterface
{
    public class UI_WheelSlot : ItemSlotInterface
    {
        public enum SelectionGraphicState { Normal, Highlighted }

        [BHeader("Item Wheel Slot")]

        [SerializeField]
        private Image m_SelectionGraphic = null;

        [SerializeField]
        private Color m_SelectionGraphicColor = Color.white;

        [SerializeField]
        private Color m_SelectionGraphicHighlightedColor = Color.gray;


        public void SetSlotHighlights(SelectionGraphicState state)
        {
            if (state == SelectionGraphicState.Normal)
            {
                m_SelectionGraphic.enabled = m_Selected;
                m_SelectionGraphic.color = m_SelectionGraphicColor;
            }
            else if (state == SelectionGraphicState.Highlighted)
            {
                m_SelectionGraphic.enabled = true;
                m_SelectionGraphic.color = m_SelectionGraphicHighlightedColor;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_SelectionGraphic.enabled = false;
        }
    }
}