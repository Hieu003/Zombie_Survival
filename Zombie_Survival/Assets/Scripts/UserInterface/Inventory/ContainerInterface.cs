using UnityEngine;
using UnityEngine.EventSystems;

namespace HQFPSWeapons.UserInterface
{
	public abstract class ContainerInterface<T> : UserInterfaceBehaviour where T: Slot
	{
		public T[] SlotInterfaces { get { return m_SlotInterfaces; } }
	
		public readonly Message<T> SlotInterfaceRefresh = new Message<T>();
		//public readonly Message AllSlotsDeselected = new Message();

		public readonly Message<T, PointerEventData> SlotPointerDown = new Message<T, PointerEventData>();
		public readonly Message<T, PointerEventData> SlotPointerUp = new Message<T, PointerEventData>();

		public readonly Message<PointerEventData, T> SlotBeginDrag = new Message<PointerEventData, T>();
		public readonly Message<PointerEventData, T> SlotDrag = new Message<PointerEventData, T>();
		public readonly Message<PointerEventData, T> SlotEndDrag = new Message<PointerEventData, T> ();

		[SerializeField]
		protected ItemSlotInterface m_SlotTemplate = null;

		[SerializeField]
		private RectTransform m_SlotsParent = null;

		[SerializeField]
		private int m_DefaultSlotCount = 1;

		[SerializeField]
		[Tooltip("If enabled, the template will be disabled after the slots are generated.")]
		private bool m_DisableTemplateAfterGen = false;

		protected T[] m_SlotInterfaces;
		protected T m_Selected;


		public virtual bool GenerateSlots()
		{
			return GenerateSlots(m_DefaultSlotCount);
		}

		public virtual bool GenerateSlots(int count)
		{
			if(m_SlotTemplate == null)
			{
				Debug.LogError("No slot template is provided, can't generate any slots.", gameObject);
				return false;
			}

			if(m_SlotsParent == null)
				Debug.LogWarning("The slots parent is not assigned. Will parent them under this object.", gameObject);

			// Remove listeners from the old slots
			RemoveListeners(m_SlotInterfaces);

			var parent = m_SlotsParent == null ? transform : m_SlotsParent;
			int childCount = parent.childCount;

			// Destroy the old slots
			for(int i = 0;i < childCount;i ++)
			{
				var child = parent.GetChild(parent.childCount - 1);
				if(child != m_SlotTemplate.transform && child.GetComponent<T>())
					DestroyImmediate(child.gameObject);
			}

			// Make sure the slot template is active, so we don't spawn disabled slots
			bool slotTemplateActive = m_SlotTemplate.gameObject.activeSelf;
			m_SlotTemplate.gameObject.SetActive(true);

			// Create the new slots
			m_SlotInterfaces = new T[count];

			for(int i = 0;i < count;i ++)
				m_SlotInterfaces[i] = Instantiate(m_SlotTemplate, parent) as T;

			// Add listeners to the new slots
			AddListeners(m_SlotInterfaces);

			m_SlotTemplate.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			m_SlotTemplate.gameObject.SetActive(m_DisableTemplateAfterGen ? false : slotTemplateActive);

			return true;
		}

		public void Select(int index)
		{
			if(index >= m_SlotInterfaces.Length || (m_Selected != null && m_SlotInterfaces[index] == m_Selected))
				return;

			if(m_Selected != null)
				m_Selected.Deselect();

			m_Selected = m_SlotInterfaces[index];
			m_Selected.Select();

			//SelectedSlotChanged.Send(m_Selected);
		}

		public void Select(Slot slot)
		{
			if((m_Selected != null && slot == m_Selected))
				return;

			for(int i = 0;i < m_SlotInterfaces.Length;i++)
				if(m_SlotInterfaces[i] == slot)
				{
					Select(i);
					return;
				}
		}

		public void DeselectAll()
		{
			for(int i = 0;i < m_SlotInterfaces.Length;i++)
				m_SlotInterfaces[i].Deselect();

			if(m_Selected != null)
			{
				m_Selected = null;
				//AllSlotsDeselected.Send();
			}
		}

		private void On_PointerDown()
        {
            //			if(m_Selected != null && m_PanelBody != null && !Manager.RectangleContainsScreenPoint(m_PanelBody))
            //			{
            //				m_Selected.Deselect();
            //				m_Selected = null;
            //
            //				//AllSlotsDeselected.Send();
            //			}
        }

        private void RemoveListeners(Slot[] slotInterfaces)
		{
			if(!Application.isPlaying || slotInterfaces == null)
				return;

			for(int i = 0;i < slotInterfaces.Length;i ++)
			{
				slotInterfaces[i].Refresh.RemoveListener(OnSlotInterfaceRefresh);	
				slotInterfaces[i].PointerDown.RemoveListener(OnPointerDownOnSlot);
				slotInterfaces[i].PointerUp.RemoveListener(OnPointerUpOnSlot);
			}
		}

		private void AddListeners(Slot[] slotInterfaces)
		{
			if(!Application.isPlaying || slotInterfaces == null)
				return;

			for(int i = 0;i < slotInterfaces.Length;i ++)
			{
				slotInterfaces[i].Refresh.AddListener(OnSlotInterfaceRefresh);
				slotInterfaces[i].PointerDown.AddListener(OnPointerDownOnSlot);
				slotInterfaces[i].PointerUp.AddListener(OnPointerUpOnSlot);

				slotInterfaces[i].BeginDrag.AddListener(OnSlotBeginDrag);
				slotInterfaces[i].Drag.AddListener(OnSlotDrag);
				slotInterfaces[i].EndDrag.AddListener(OnSlotEndDrag);
			}
		}

		private void OnSlotBeginDrag(PointerEventData data, Slot slotInterface)
		{
			SlotBeginDrag.Send(data, slotInterface as T);
		}

		private void OnSlotDrag(PointerEventData data, Slot slotInterface)
		{
			SlotDrag.Send(data, slotInterface as T);
		}

		private void OnSlotEndDrag(PointerEventData data, Slot slotInterface)
		{
			SlotEndDrag.Send(data, slotInterface as T);
		}

		private void OnSlotInterfaceRefresh(Slot slotInterface)
		{
			SlotInterfaceRefresh.Send(slotInterface as T);
		}

		private void OnPointerDownOnSlot(Slot slotInterface, PointerEventData data)
		{
			SlotPointerDown.Send(slotInterface as T, data);
		}

		private void OnPointerUpOnSlot(Slot slotInterface, PointerEventData data)
		{
//			if(data.pointerCurrentRaycast.gameObject != null && data.pointerCurrentRaycast.gameObject == slotInterface.gameObject)
//				Inventory.CurrentSelectedSlot.Set(slotInterface);
			
			SlotPointerUp.Send(slotInterface as T, data);
		}
	}
}