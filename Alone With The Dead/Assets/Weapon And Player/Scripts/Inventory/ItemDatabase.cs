using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// Represents an asset that stores all the user-defined items.
	/// </summary>
	[CreateAssetMenu(menuName = "HQ FPS Weapons Pack/Item Database")]
	public class ItemDatabase : ScriptableObject
	{
		public static ItemDatabase Default 
		{
			get 
			{
				if(m_Default == null)
				{
					var allDatabases = Resources.LoadAll<ItemDatabase>("");
					if(allDatabases != null && allDatabases.Length > 0)
						m_Default = allDatabases[0];
				}

				return m_Default;
			}
		}

		private static ItemDatabase m_Default;

		public ItemCategory[] Categories { get { return m_Categories; } }

		[SerializeField]
		private ItemCategory[] m_Categories = null;

		[SerializeField]
		private ItemProperty.Definition[] m_ItemProperties = null;

		private Dictionary<string, ItemData> m_Items = new Dictionary<string, ItemData>();


		public ItemData GetItemData(string name)
		{
			ItemData itemData;

			if(m_Items.TryGetValue(name, out itemData))
				return itemData;
			else
				return null;
		}

		public bool TryGetItem(string name, out ItemData itemData)
		{
			return m_Items.TryGetValue(name, out itemData);
		}

		public List<string> GetAllItemNames()
		{
			List<string> names = new List<string>();

			for(int i = 0;i < m_Categories.Length;i ++)
			{
				var category = m_Categories[i];
				for(int j = 0;j < category.Items.Length;j ++)
					names.Add(category.Items[j].Name);
			}

			return names;
		}

		public List<string> GetAllItemNamesFull()
		{
			List<string> names = new List<string>();

			for(int i = 0;i < m_Categories.Length;i ++)
			{
				var category = m_Categories[i];
				for(int j = 0;j < category.Items.Length;j ++)
					names.Add(m_Categories[i].Name + "/" + category.Items[j].Name);
			}

			return names;
		}

		public List<string> GetAllCategoryNames()
		{
			List<string> names = new List<string>();

			for(int i = 0;i < m_Categories.Length;i ++)
				names.Add(m_Categories[i].Name);

			return names;
		}

		public string[] GetAllPropertyNames()
		{
			string[] names = new string[m_ItemProperties.Length];

			for(int i = 0;i < m_ItemProperties.Length;i ++)
				names[i] = m_ItemProperties[i].Name;

			return names;
		}

		public ItemProperty.Definition GetPropertyByName(string name)
		{
			for(int i = 0;i < m_ItemProperties.Length;i ++)
				if(m_ItemProperties[i].Name == name)
					return m_ItemProperties[i];

			return default(ItemProperty.Definition);
		}

		public ItemProperty.Definition GetPropertyAtIndex(int index)
		{
			if(index >= m_ItemProperties.Length)
				return default(ItemProperty.Definition);
			else
				return m_ItemProperties[index];
		}

		public ItemCategory GetCategoryByName(string name)
		{
			for(int i = 0;i < m_Categories.Length;i ++)
				if(m_Categories[i].Name == name)
					return m_Categories[i];

			return null;
		}

		public ItemCategory GetRandomCategory()
		{
			return m_Categories[Random.Range(0, m_Categories.Length)];
		}

		public int GetItemCount()
		{
			int count = 0;

			for(int c = 0;c < m_Categories.Length;c ++)
				count += m_Categories[c].Items.Length;

			return count;
		}

		private void OnEnable()
		{
			GenerateDictionaries();
		}

		private void OnValidate()
		{
			foreach (var category in m_Categories)
			{
				for (int j = 0; j < category.Items.Length; j++)
				{
					category.Items[j].Category = category.Name;
				}
			}

			GenerateDictionaries();
		}

		private void GenerateDictionaries()
		{
			m_Items = new Dictionary<string, ItemData>();

			for(int c = 0;c < m_Categories.Length;c ++)
			{
				var category = m_Categories[c];

				for(int i = 0;i < category.Items.Length;i ++)
				{
					var item = category.Items[i];
					if(!m_Items.ContainsKey(item.Name))
					{
						m_Items.Add(item.Name, item);
					}
				}
			}
		}
	}
}