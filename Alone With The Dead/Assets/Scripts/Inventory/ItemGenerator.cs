using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	[Serializable]
	public class ItemGenerator
	{
		[SerializeField]
		private Method m_Method = Method.RandomItemFromCategory;

		[SerializeField]
		[DatabaseCategory]
		private string m_Category = null;

		[SerializeField]
		[DatabaseItem]
		private string m_Item = null;

		[SerializeField]
		[Clamp(1, 999)]
		private int m_CountMin = 1;

		[SerializeField]
		[Clamp(1, 999)]
		private int m_CountMax = 1;


		public SaveableItem GenerateItem()
		{
			if(m_Method == Method.Empty)
				return null;

			var database = ItemDatabase.Default;
			ItemData itemData = null;

			if(m_Method == Method.CustomItem)
				database.TryGetItem(m_Item, out itemData);
			else
			{
				ItemCategory category = null;

				if(m_Method == Method.RandomItem)
					category = database.GetRandomCategory();
				else if(m_Method == Method.RandomItemFromCategory)
					category = database.GetCategoryByName(m_Category);

				if(category != null && category.Items.Length > 0)
					itemData = category.Items[Random.Range(0, category.Items.Length)];
			}

			if(itemData != null)
				return new SaveableItem(itemData, Random.Range(m_CountMin, m_CountMax + 1));

			return null;
		}


		// ------------------ Internal ---------------
		public enum Method 
		{
			CustomItem,
			RandomItemFromCategory,
			RandomItem,
			Empty
		}
	}
}