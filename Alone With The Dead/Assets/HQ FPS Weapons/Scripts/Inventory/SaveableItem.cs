using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// A SavableItem is an instance of an item, which can have it's own properties, vs ItemData which is just the data for an item, just the definition.
	/// </summary>
	[Serializable]
	public class SaveableItem : IDeserializationCallback
	{
		[NonSerialized]
		public Message<ItemProperty.Value> PropertyChanged = new Message<ItemProperty.Value>();

		[NonSerialized]
		public Message StackChanged = new Message();

		public ItemData Data { get { return ItemDatabase.Default.GetItemData(m_Name); } }
		public string Name { get { return m_Name; } }
		public int CurrentStackSize { get { return m_CurrentStackSize; } set { m_CurrentStackSize = value; StackChanged.Send(); } }
		public ItemPropertyList Properties { get { return m_Properties; } }

		private string m_Name;
		private int m_CurrentStackSize;
		private ItemPropertyList m_Properties;


		public static implicit operator bool(SaveableItem item) 
		{
			return item != null;
		}

		/// <summary>
		/// 
		/// </summary>
		public SaveableItem(ItemData data, int currentInStack = 1, ItemPropertyList customProperties = null)
		{
			m_Name = data.Name;

			CurrentStackSize = Mathf.Clamp(currentInStack, 1, data.StackSize);

//			if(data.IsContainer)
//				m_Container = data.GenerateContainer(null);

			if(customProperties != null)
				m_Properties = CloneProperties(customProperties);
			else
				m_Properties = CloneProperties(data.Properties);

			for(int i = 0;i < m_Properties.Count;i ++)
				m_Properties[i].Changed.AddListener(On_PropertyChanged);
		}
			
		public void OnDeserialization(object sender)
		{
			PropertyChanged = new Message<ItemProperty.Value>();
			StackChanged = new Message();

			var itemDatabase = ItemDatabase.Default;

			if(itemDatabase)
			{
				ItemData data;
				if(itemDatabase.TryGetItem(Name, out data))
				{
					for(int i = 0;i < m_Properties.Count;i ++)
					{
						m_Properties[i].Changed = new Message<ItemProperty.Value>();
						m_Properties[i].Changed.AddListener(On_PropertyChanged);
					}
				}
				else
					Debug.LogErrorFormat("[SavableItem] - This item couldn't be initialized and will not function properly. No item with the name {0} was found in the database!", Name);
			}
			else
				Debug.LogError("[SavableItem] - This item couldn't be initialized and will not function properly. The item database provided is null!");
		}

		public string GetDescription(int index)
		{
			string description = string.Empty;
			if(index > -1 && Data.Descriptions.Length > index)
			{
				try
				{
					description = string.Format(Data.Descriptions[index].Description, m_Properties.ToArray());
				}
				catch
				{
					Debug.LogError("[SavableItem] - You tried to access a property through the item description, but the property doesn't exist. The item name is: " + Name);
				}
			}

			return description;
		}

		public bool HasProperty(string name)
		{
			for(int i = 0;i < m_Properties.Count;i ++)
				if(m_Properties[i].Name == name)
					return true;

			return false;
		}

		/// <summary>
		/// Use this if you are sure the item has this property.
		/// </summary>
		public ItemProperty.Value GetProperty(string name)
		{
			ItemProperty.Value propertyValue = null;

			for(int i = 0;i < m_Properties.Count;i ++)
				if(m_Properties[i].Name == name)
				{
					propertyValue = m_Properties[i];
					break;
				}

			return propertyValue;
		}

		/// <summary>
		/// Use this if you are NOT sure the item has this property.
		/// </summary>
		public bool FindProperty(string name, out ItemProperty.Value propertyValue)
		{
			propertyValue = null;

			for(int i = 0;i < m_Properties.Count;i ++)
				if(m_Properties[i].Name == name)
				{
					propertyValue = m_Properties[i];
					return true;
				}

			return false;
		}

		private ItemPropertyList CloneProperties(ItemPropertyList properties)
		{
			List<ItemProperty.Value> list = new List<ItemProperty.Value>();
			for(int i = 0;i < properties.Count;i ++)
				list.Add(properties[i].GetClone());

			return new ItemPropertyList() { List = list };
		}

		private void On_PropertyChanged(ItemProperty.Value propertyValue)
		{
			PropertyChanged.Send(propertyValue);
		}
	}
}