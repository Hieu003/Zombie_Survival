using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HQFPSWeapons.UserInterface
{
    public class ItemWheel : UserInterfaceBehaviour
    {
        public enum ItemWheelState { SelectItems, InsertItems}

        [BHeader("GENERAL", true)]

        [SerializeField]
        private Panel m_Panel = null;

        [SerializeField]
        private RectTransform m_WheelArrow = null;

        [Range(0f, 2f)]
        private float m_WheelToggleCooldown = 0.25f;

        [SerializeField]
        private string m_ContainerName = string.Empty;

        [SerializeField]
        private float m_Sensitivity = 3f;

        [SerializeField]
        private float m_Range = 3f;

        [SerializeField]
        private Text m_DescriptionText = null;

        [SerializeField]
        private Text m_ItemNameText = null;

        [BHeader("Slot Positioning...")]

        [SerializeField]
        private float m_RadialSpacing = 45f;

        [SerializeField]
        private float m_RadialDistance = 255f;

        [SerializeField]
        private float m_RadialOffset = 90f;

        private Dictionary<UI_WheelSlot, ItemSlot> m_SlotDictionary = new Dictionary<UI_WheelSlot, ItemSlot>();
        private UI_WheelSlot[] m_WheelSlots;

        private ItemContainer m_HolsterContainer;

        private int m_LastHighlightedSlot = -1;
        private int m_LastSelectedSlot = -1;

        private Vector2 m_CursorPos;

        private Vector2 m_DirectionOfSelection;

        private float m_NextTimeCanToggleWheel;

        private bool m_IsVisible;

        private ItemWheelState m_ItemWheelState;


        public void SetItemWheelState(int state) 
        {
            if (state == 0)
                m_ItemWheelState = ItemWheelState.SelectItems;
            else if (state == 1)
            {
                m_ItemWheelState = ItemWheelState.InsertItems;

                foreach (UI_WheelSlot slot in m_WheelSlots)
                {
                    slot.Deselect();
                    slot.SetSlotHighlights(UI_WheelSlot.SelectionGraphicState.Normal);
                }

                m_LastHighlightedSlot = -1;
                m_LastSelectedSlot = -1;
            }
        }

        public void PositionSlots()
        {
            var wheelSlots = GetComponentsInChildren<UI_WheelSlot>();

            for(int i = 0;i < wheelSlots.Length;i++)
            {
                float angle = Mathf.Deg2Rad * (m_RadialSpacing * i + m_RadialOffset);
                RectTransform rectTransf = wheelSlots[i].GetComponent<RectTransform>();

                Vector2 positionOnCircle = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * m_RadialDistance;
                rectTransf.anchoredPosition = positionOnCircle;
                rectTransf.up = positionOnCircle;

                // Icon
                RectTransform icon = rectTransf.Find("Icon").GetComponent<RectTransform>();
                icon.up = Vector3.up;
            }
        }

        public override void OnAttachment()
        {
            m_HolsterContainer = Player.Inventory.GetContainerWithName(m_ContainerName);

            if(m_HolsterContainer != null)
            {
                for(int i = 0;i < m_HolsterContainer.Count;i++)
                {
                    m_SlotDictionary.Add(m_WheelSlots[i], m_HolsterContainer[i]);
                    m_WheelSlots[i].LinkToSlot(m_HolsterContainer[i]);
                }
            }

            Manager.ItemWheel.AddStartTryer(TryStart_ItemWheelInspection);
            Manager.ItemWheel.AddStopTryer(TryStop_ItemWheelInspection);

            m_HolsterContainer.Changed.AddListener(ChangedHolsterContainer);
        }   

        private void ChangedHolsterContainer(ItemSlot slot) 
        {
            if (slot.Item != null)
            {
                int indexOfChangedSlot = IndexOfSlot(slot);

                HandleSlotHighlighting(indexOfChangedSlot);
                HandleSlotSelection(indexOfChangedSlot);
            }
        }

        private int IndexOfSlot(ItemSlot slot)
        {
            for (int i = 0; i < m_HolsterContainer.Slots.Length; i++)
            {
                if (m_HolsterContainer[i] == slot)
                    return i;
            }

            return -1;
        }

        private void Update()
        {
            if (!m_Panel.IsVisible)
            {
                m_IsVisible = false;
                return;
            }

            if (!m_IsVisible)
            {
                if (m_LastSelectedSlot != -1)
                {
                    TryShowSlotInfo(m_WheelSlots[m_LastSelectedSlot]);

                    if (!m_WheelSlots[m_LastSelectedSlot].HasItem)
                    {
                        m_WheelSlots[m_LastSelectedSlot].Deselect();
                        m_WheelSlots[m_LastSelectedSlot].SetSlotHighlights(UI_WheelSlot.SelectionGraphicState.Normal);
                        m_LastSelectedSlot = -1;
                    }
                }

                m_IsVisible = true;
            }

            if (m_ItemWheelState == ItemWheelState.InsertItems)
                return;

            int highlightedSlot = GetHighlightedSlot();

            if(highlightedSlot != m_LastHighlightedSlot)
                HandleSlotHighlighting(highlightedSlot);
        }

        private bool TryStart_ItemWheelInspection()
        {
            if(!Player.Aim.Active && Time.time > m_NextTimeCanToggleWheel && !Player.Healing.Active)
            {
                m_Panel.TryShow(true);
                m_NextTimeCanToggleWheel = Time.time + m_WheelToggleCooldown;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                return true;
            }

            return false;
        }

        private bool TryStop_ItemWheelInspection()
        {
            if(Time.time > m_NextTimeCanToggleWheel)
            {
                m_Panel.TryShow(false);
                HandleSlotSelection(m_LastHighlightedSlot);
                m_NextTimeCanToggleWheel = Time.time + m_WheelToggleCooldown;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                return true;
            }

            return false;
        }

        private void Awake()
        {
            m_ItemWheelState = ItemWheelState.SelectItems;
            m_WheelSlots = GetComponentsInChildren<UI_WheelSlot>();
        }

        private int GetHighlightedSlot()
        {
            Vector2 directionOfSelection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).normalized * m_Range;

            if(directionOfSelection != Vector2.zero) 
                m_DirectionOfSelection = Vector2.Lerp(m_DirectionOfSelection, directionOfSelection, Time.deltaTime * m_Sensitivity);

            m_CursorPos = m_DirectionOfSelection;
            
            float angle = -Vector2.SignedAngle(Vector2.up, m_CursorPos);

            if (angle < 0)
                angle = 360f - Mathf.Abs(angle);

            m_WheelArrow.rotation = Quaternion.Euler(0f, 0f, -angle);

            angle = 360f - angle;

            float angleBetweenSlots = 360f / m_WheelSlots.Length;

            angle -= angleBetweenSlots / 2;

            if (angle > 360f)
                angle = angle - 360f;

            if (!(angle + angleBetweenSlots / 2 > 360 - angleBetweenSlots / 2))
                return Mathf.Clamp(Mathf.RoundToInt((angle + angleBetweenSlots / 2) / angleBetweenSlots), 0, m_WheelSlots.Length - 1);
            else
                return 0;
        }

        private void HandleSlotHighlighting(int highlightedSlot)
        {
            m_WheelSlots[highlightedSlot].SetSlotHighlights(UI_WheelSlot.SelectionGraphicState.Highlighted);
            m_WheelSlots[highlightedSlot].Select();

            if (m_LastHighlightedSlot != -1)
            {
                if (m_LastSelectedSlot != m_LastHighlightedSlot)
                    m_WheelSlots[m_LastHighlightedSlot].Deselect();

                m_WheelSlots[m_LastHighlightedSlot].SetSlotHighlights(UI_WheelSlot.SelectionGraphicState.Normal);
            }

            m_LastHighlightedSlot = highlightedSlot;   

            TryShowSlotInfo(m_WheelSlots[highlightedSlot]);
        }

        private void HandleSlotSelection(int highlightedSlot)
        {
            int currentSelectedSlot = highlightedSlot;

            var slot = m_SlotDictionary[m_WheelSlots[highlightedSlot]];
            
            Player.EquipItem.Try(slot.Item, false);

            m_HolsterContainer.SelectedSlot = currentSelectedSlot;

            //Selection Graphics
            if (currentSelectedSlot != m_LastSelectedSlot)
            {
                if (m_LastSelectedSlot != -1)
                {
                    if (m_WheelSlots[currentSelectedSlot].Item == null)
                    {
                        m_WheelSlots[m_LastSelectedSlot].Deselect();
                        m_WheelSlots[m_LastSelectedSlot].SetSlotHighlights(UI_WheelSlot.SelectionGraphicState.Normal);

                        m_LastSelectedSlot = -1;
                    }
                    else
                    {
                        m_WheelSlots[currentSelectedSlot].Select();
                        m_WheelSlots[currentSelectedSlot].SetSlotHighlights(UI_WheelSlot.SelectionGraphicState.Highlighted);

                        m_WheelSlots[m_LastSelectedSlot].Deselect();
                        m_WheelSlots[m_LastSelectedSlot].SetSlotHighlights(UI_WheelSlot.SelectionGraphicState.Normal);

                        m_LastSelectedSlot = currentSelectedSlot;
                    }          
                }
                else if(m_WheelSlots[currentSelectedSlot].Item != null)
                {
                    m_WheelSlots[currentSelectedSlot].Select();
                    m_WheelSlots[currentSelectedSlot].SetSlotHighlights(UI_WheelSlot.SelectionGraphicState.Highlighted);

                    m_LastSelectedSlot = currentSelectedSlot;
                }
            }
        }

        private void TryShowSlotInfo(UI_WheelSlot slot)
        {
            ItemSlot itemSlot;

            if (m_SlotDictionary.TryGetValue(slot, out itemSlot))
            {
                if (itemSlot != null && itemSlot.HasItem)
                {
                    m_ItemNameText.text = itemSlot.Item.Name;

                    if(itemSlot.Item.Data.Descriptions.Length > 0)
                        m_DescriptionText.text = itemSlot.Item.Data.Descriptions[0].Description;
                }
                else
                {
                    m_ItemNameText.text = "";
                    m_DescriptionText.text = "";
                }
            }
        }
    }
}
