using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace HQFPSWeapons
{
	[Serializable]
	public class ItemContainer : IEnumerable
	{
		public ItemSlot this[int i] { get { return m_Slots[i]; } set { m_Slots[i] = value; } }

		// A little hack for now
		public int SelectedSlot { get; set; }

		/// <summary>Slot count.</summary>
		public int Count { get { return m_Slots.Length; } }

		public ItemSlot[] Slots { get { return m_Slots; } }

		public string Name { get { return m_Name; } }

		public ItemContainerFlags Flag { get { return m_Flag; } }

		public Transform Parent { get { return m_Parent; } }

		public bool IsExpanded { get { return m_IsExpanded; } set { m_IsExpanded = value; } }

		[NonSerialized]
		public Message<ItemSlot> Changed = new Message<ItemSlot>();

		[NonSerialized]
		private Transform m_Parent = null;

		private string m_Name;
		private ItemContainerFlags m_Flag;
		private ItemSlot[] m_Slots;
		private string[] m_ValidCategories;
		private string[] m_RequiredProperties;
		private bool m_IsExpanded = true;


		public ItemContainer(string name, int size, Transform parent, ItemContainerFlags flag, string[] validCategories = null, string[] validProperties = null)
		{
			m_Name = name;
			m_Slots = new ItemSlot[size];
			for(int i = 0;i < m_Slots.Length;i ++)
			{
				m_Slots[i] = new ItemSlot();
				m_Slots[i].Changed.AddListener(OnSlotChanged);
			}

			m_Flag = flag;
			m_ValidCategories = validCategories;
			m_RequiredProperties = validProperties;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_Slots.GetEnumerator();
		}
			
		public int AddItem(ItemData itemData, int amount, ItemPropertyList customProperties = null)
		{
			if(itemData == null || !AllowsItem(itemData))
				return 0;

			int amountInInventory = GetItemCount(itemData.Name);

			if (amountInInventory >= itemData.StackSize)
				return 0;
			else
				amount = Mathf.Min(itemData.StackSize - amountInInventory, amount);

			int added = 0;

			// Go through each slot and see where we can add the item(s)
			for(int i = 0;i < m_Slots.Length;i ++)
			{
				added += AddToSlot(m_Slots[i], itemData, amount, customProperties);

				// We've added all the items, we can stop now
				if(added == amount)
					return added;
			}

			return added;
		}

		public int AddItem(string name, int amount, ItemPropertyList customProperties = null)
		{
			ItemData itemData;
			if(!ItemDatabase.Default.TryGetItem(name, out itemData) || !AllowsItem(itemData))
				return 0;

			return AddItem(itemData, amount, customProperties);
		}

		public bool AddItem(SaveableItem item)
		{
			if(AllowsItem(item))
			{
				if(item.CurrentStackSize > 1)
					return AddItem(item.Data, item.CurrentStackSize, item.Properties) > 0;
				else
				{
					if(m_Slots != null && m_Slots.Length > 0 && !m_Slots[Mathf.Clamp(SelectedSlot, 0, m_Slots.Length - 1)].HasItem)
					{
						m_Slots[Mathf.Clamp(SelectedSlot, 0, m_Slots.Length - 1)].SetItem(item);
						return true;
					}
					else
					{
						// Go through each slot and see where we can add the item
						for (int i = 0; i < m_Slots.Length; i++)
						{
							if (!m_Slots[i].HasItem)
							{
								m_Slots[i].SetItem(item);
								return true;
							}
						}
					}

					return false;
				}
			}
			else
				return false;
		}

		public int RemoveItem(string name, int amount)
		{
			int removed = 0;

			for(int i = 0;i < m_Slots.Length;i ++)
			{
				if(m_Slots[i].HasItem && m_Slots[i].Item.Name == name)
				{
					removed += m_Slots[i].RemoveFromStack(amount - removed);

					// We've removed all the items, we can stop now
					if(removed == amount)
						return removed;
				}
			}

			return removed;
		}

		public bool RemoveItem(SaveableItem item)
		{
			for(int i = 0;i < m_Slots.Length;i ++)
				if(m_Slots[i].Item == item)
				{
					m_Slots[i].SetItem(null);
					return true;
				}

			return false;
		}

		public bool ContainsItem(SaveableItem item)
		{
			for(int i = 0;i < m_Slots.Length;i ++)
				if(m_Slots[i].Item == item)
					return true;

			return false;
		}

		public int GetItemCount(string name)
		{
			int count = 0;

			for(int i = 0;i < m_Slots.Length;i ++)
			{
				if(m_Slots[i].HasItem && m_Slots[i].Item.Name == name)
					count += m_Slots[i].Item.CurrentStackSize;
			}

			return count;
		}

		public bool AllowsItem(ItemData itemData)
		{
			bool isFromValidCategories = m_ValidCategories == null || m_ValidCategories.Length == 0;
			bool hasValidProperties = true;

			if(m_ValidCategories != null)
			{
				for(int i = 0;i < m_ValidCategories.Length;i ++)
				{
					if(m_ValidCategories[i] == itemData.Category)
						isFromValidCategories = true;
				}
			}

			if(m_RequiredProperties != null)
			{
				for(int i = 0;i < m_RequiredProperties.Length;i ++)
				{
					bool hasProperty = false;
					for(int p = 0;p < itemData.Properties.Length;p ++)
					{
						if(itemData.Properties[p].Name == m_RequiredProperties[i])
						{
							hasProperty = true;
							break;
						}
					}

					if(!hasProperty)
					{
						hasValidProperties = false;
						break;
					}
				}
			}

			return isFromValidCategories && hasValidProperties;
		}

		public bool AllowsItem(SaveableItem item)
		{
			bool isFromValidCategories = m_ValidCategories == null || m_ValidCategories.Length == 0;
			bool hasRequiredProperties = true;

			if(m_ValidCategories != null)
			{
				for(int i = 0;i < m_ValidCategories.Length;i ++)
				{
					if(m_ValidCategories[i] == item.Data.Category)
						isFromValidCategories = true;
				}
			}

			if(m_RequiredProperties != null)
			{
				for(int i = 0;i < m_RequiredProperties.Length;i ++)
				{
					if(!item.HasProperty(m_RequiredProperties[i]))
						hasRequiredProperties = false;
				}
			}

			return isFromValidCategories && hasRequiredProperties;
		}

		public int GetPositionOfItem(SaveableItem item)
		{
			for(int i = 0; i < m_Slots.Length; i ++)
				if(m_Slots[i].Item == item)
					return i;

			return -1;
		}

		public bool ContainerIsFull () 
		{
			foreach (var slot in m_Slots)
			{
				if (!slot.HasItem)
					return false;
			}

			return true;
		}

		private int AddToSlot(ItemSlot slot, ItemData itemData, int amount, ItemPropertyList customProperties = null)
		{
			if(slot.HasItem && itemData.Name != slot.Item.Name)
				return 0;

			bool wasEmpty = false;

			if(!slot.HasItem)
			{
				slot.SetItem(new SaveableItem(itemData, 1, customProperties));
				amount --;
				wasEmpty = true;
			}

			int addedToStack = slot.AddToStack(amount);

			return addedToStack + (wasEmpty ? 1 : 0);
		}

		private void OnSlotChanged(ItemSlot slot)
		{
			try
			{
				Changed.Send(slot);
			}
			catch { };
		}
	}

}