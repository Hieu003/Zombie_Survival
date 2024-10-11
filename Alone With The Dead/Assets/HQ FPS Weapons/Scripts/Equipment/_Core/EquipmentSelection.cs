using UnityEngine;

namespace HQFPSWeapons
{
    public class EquipmentSelection : PlayerComponent
    {
        [SerializeField]
        private string m_HolsterContainerName = "Holster";

        [Space]

        [SerializeField]
        [Range(0, 9)]
        private int m_FirstSelected = 0;

        [BHeader("Navigation")]

        [SerializeField]
        private bool m_EnableScrolling = true;

        [SerializeField]
        [ShowIf("m_EnableScrolling", true)]
        [Clamp(0f, 1f)]
        private float m_ScrollThreshold = 0.3f;

        [SerializeField]
        [ShowIf("m_EnableScrolling", true)]
        [Clamp(0f, 1f)]
        private float m_ScrollPause = 0.3f;

        [SerializeField]
        private bool m_SelectByDigits = true;

        [SerializeField]
        [ShowIf("m_SelectByDigits", true)]
        [Clamp(0f, 1f)]
        private float m_SelectThreshold = 0.3f;

        private ItemContainer m_HolsterContainer;
        private ItemSlot m_SelectedSlot;

        private int m_CurrentScrollIndex;
        private float m_CurScrollValue;
        private float m_NextTimeCanSelect;


        private void Start()
        {
            m_HolsterContainer = Player.Inventory.GetContainerWithName(m_HolsterContainerName);

            if(m_HolsterContainer != null)
            {
                m_HolsterContainer.Changed.AddListener(OnHolsterChanged);

                if (Player.EquippedItem.Get() == null)
                    TrySelectSlot(m_FirstSelected);
                else
                    TrySelectSlot(m_HolsterContainer.GetPositionOfItem(Player.EquippedItem.Get()));
            }

            m_HolsterContainer.Changed.AddListener(ChangedHolsterContainer);
        }

        private void ChangedHolsterContainer(ItemSlot slot)
        {
             int indexOfChangedSlot = IndexOfSlot(slot);

             TrySelectSlot(indexOfChangedSlot);
        }

        private void Update()
        {
            if(Player == null || Player.Healing.Active)
                return;

            if(m_SelectByDigits && Input.anyKeyDown && m_NextTimeCanSelect < Time.time)
            {
                if(int.TryParse(Input.inputString, out int keyNumber))
                    TrySelectSlot(keyNumber - 1);

                m_NextTimeCanSelect = Time.time + m_SelectThreshold;
            }

            if(m_EnableScrolling && !Player.Pause.Active)
            {
                var playerScrollValue = Player.ScrollValue.Get();

                m_CurScrollValue = Mathf.Clamp(m_CurScrollValue + playerScrollValue, -m_ScrollThreshold, m_ScrollThreshold);

                if(Mathf.Abs(m_CurScrollValue - m_ScrollThreshold * Mathf.Sign(playerScrollValue)) < Mathf.Epsilon && m_NextTimeCanSelect < Time.time)
                {
                    m_CurScrollValue = 0f;

                    int lastScrollIndex = m_CurrentScrollIndex;

                    while(true)
                    {
                        m_CurrentScrollIndex = (int)Mathf.Repeat(m_CurrentScrollIndex + (playerScrollValue >= 0f ? 1 : -1), m_HolsterContainer.Slots.Length);

                        if (m_HolsterContainer.Slots[m_CurrentScrollIndex].HasItem)
                        {
                            TrySelectSlot(m_CurrentScrollIndex);

                            break;
                        }

                        if (lastScrollIndex == m_CurrentScrollIndex)
                            break;
                    }

                    m_NextTimeCanSelect = Time.time + m_ScrollPause;
                }
            }
        }

        private void OnHolsterChanged(ItemSlot slot)
        {
            if (slot == m_SelectedSlot)
            {
                if (Player.EquippedItem.Get() != null && !m_SelectedSlot.HasItem)
                    Player.EquipItem.Try(null, true);
                else if (Player.EquippedItem.Get() == null && m_SelectedSlot.HasItem)
                    Player.EquipItem.Try(m_SelectedSlot.Item, false);
                else if (Player.EquippedItem.Get() != null && m_SelectedSlot.HasItem && m_SelectedSlot.Item != Player.EquippedItem.Get())
                    Player.EquipItem.Try(m_SelectedSlot.Item, false);
            }
        }

        private void TrySelectSlot(int index)
        {
            TrySelectSlot(m_HolsterContainer.Slots[Mathf.Clamp(index, 0, m_HolsterContainer.Slots.Length - 1)]);
        }

        private void TrySelectSlot(ItemSlot slot)
        {
            m_SelectedSlot = slot;
    
           Player.EquipItem.Try(slot.Item, false);

            m_HolsterContainer.SelectedSlot = IndexOfSlot(slot);
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
    }
}