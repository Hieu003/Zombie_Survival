using UnityEngine;
using UnityEngine.EventSystems;

namespace HQFPSWeapons.UserInterface
{
	public class SlotGroup : MonoBehaviour
	{
		public Message<Slot> SlotSelected = new Message<Slot>();

		[SerializeField]
		private bool m_FindChildrenAtStart = true;

        [SerializeField]
        private bool m_SelectFirstChildAtStart = true;

        [SerializeField]
        private RectTransform m_SelectionFrame = null;

		private Slot[] m_Slots;
		private Slot m_SelectedSlot;

        private Slot m_PointerDownSlot;


		public void SetSlots(Slot[] slots)
		{
			if(m_Slots != null)
			{
                for(int i = 0;i < m_Slots.Length;i++)
                {
                    slots[i].PointerDown.RemoveListener(SelectSlot);
                    slots[i].PointerUp.RemoveListener(OnPointerUpOnSlot);
                }

				if(m_SelectedSlot != null)
				{
					m_SelectedSlot.Deselect();
					m_SelectedSlot = null;
				}
			}

			if(slots != null && slots.Length > 0)
			{
				m_Slots = slots;

                for(int i = 0;i < m_Slots.Length;i++)
                {
                    slots[i].PointerDown.AddListener(OnPointerDownOnSlot);
                    slots[i].PointerUp.AddListener(OnPointerUpOnSlot);
                }
			}
		}

		public void SelectSlot(Slot slot, PointerEventData data)
		{
			if(m_SelectedSlot != null)
				m_SelectedSlot.Deselect();

			slot.Select();
			m_SelectedSlot = slot;

            if(m_SelectionFrame != null)
            {
                if(!m_SelectionFrame.gameObject.activeSelf)
                    m_SelectionFrame.gameObject.SetActive(true);

                m_SelectionFrame.SetParent(slot.transform);
                m_SelectionFrame.anchoredPosition = Vector2.zero;
            }

			SlotSelected.Send(m_SelectedSlot);
		}

		public void SelectSlot(Slot slot)
		{
			SelectSlot(slot, null);
		}

        private void Start()
        {
            if (m_FindChildrenAtStart)
            {
                SetSlots(GetComponentsInChildren<Slot>());

                if (m_SelectFirstChildAtStart && m_Slots != null && m_Slots.Length != 0)
                    SelectSlot(m_Slots[0]);
            }
        }

        private void OnPointerDownOnSlot(Slot slot, PointerEventData data)
        {
            m_PointerDownSlot = slot;
           
        }

        private void OnPointerUpOnSlot(Slot slot, PointerEventData data)
        {
            GameObject objectUnderPointer = data.pointerCurrentRaycast.gameObject;
            Slot slotUnderPointer = objectUnderPointer == null ? null : objectUnderPointer.GetComponent<Slot>();

            SelectSlot(slot, data);
        }
    }
}