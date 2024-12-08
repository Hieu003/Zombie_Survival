using System;
using UnityEngine;

namespace HQFPSWeapons
{
	[Serializable]
	public class ItemSlot
	{
		/// <summary>Sent when this slot has changed (e.g. when the item has changed).</summary>
		[NonSerialized]
		public Message<ItemSlot> Changed = new Message<ItemSlot>();

		public bool HasItem { get { return Item != null; } }
		public SaveableItem Item { get { return m_Item; } }

		private SaveableItem m_Item;


		public static implicit operator bool(ItemSlot slot) 
		{
			return slot != null;
		}

		public void SetItem(SaveableItem item)
		{
			if(Item)
			{
				Item.PropertyChanged.RemoveListener(On_PropertyChanged);
				Item.StackChanged.RemoveListener(On_StackChanged);
			}

			m_Item = item;

			if(Item)
			{
				Item.PropertyChanged.AddListener(On_PropertyChanged);
				Item.StackChanged.AddListener(On_StackChanged);
			}

			Changed.Send(this);
		}

		public int RemoveFromStack(int amount)
		{
			if(!HasItem)
				return 0;

			if(amount >= Item.CurrentStackSize)
			{
				int stackSize = Item.CurrentStackSize;
				SetItem(null);

				return stackSize;
			}

			int oldStack = Item.CurrentStackSize;
			Item.CurrentStackSize = Mathf.Max(Item.CurrentStackSize - amount, 0);

			Changed.Send(this);

			return oldStack - Item.CurrentStackSize;
		}

		public int AddToStack(int amount)
		{
			if(!HasItem || Item.Data.StackSize <= 1)
				return 0;

			int oldStackCount = Item.CurrentStackSize;
			int surplus = amount + oldStackCount - Item.Data.StackSize;
			int currentStackCount = oldStackCount;

			if(surplus <= 0)
				currentStackCount += amount;
			else
				currentStackCount = Item.Data.StackSize;

			Item.CurrentStackSize = currentStackCount;

			return currentStackCount - oldStackCount;
		}

		private void On_PropertyChanged(ItemProperty.Value propertyValue)
		{
			Changed.Send(this);
		}

		private void On_StackChanged()
		{
			Changed.Send(this);
		}
	}
}
