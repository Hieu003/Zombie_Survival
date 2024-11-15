using System;
using UnityEngine;

namespace HQFPSWeapons
{
	[Serializable]
	public class ContainerGenerator
	{
		[SerializeField]
		private string m_Name = string.Empty;

		[SerializeField]
		private ItemContainerFlags m_Flag = ItemContainerFlags.AmmoPouch;

		[SerializeField]
		[Range(1, 100)]
		private int m_Size = 10;

		[SerializeField]
		[Space]
		[Reorderable]
		private ItemGeneratorList m_ItemGenerators = null;

		[Header("Item Filtering")]

		[SerializeField]
		[DatabaseCategory]
		private string[] m_ValidCategories = null;

		[SerializeField]
		[DatabaseProperty]
		private string[] m_RequiredProperties = null;


		public ItemContainer GenerateContainer(Transform parent)
		{
			var container = new ItemContainer(
				m_Name,
				m_Size,
				parent,
				m_Flag,
				m_ValidCategories,
				m_RequiredProperties);

			for(int i = 0; i < m_ItemGenerators.Count; i ++)
			{
				if(i >= container.Slots.Length)
					break;

				SaveableItem generatedItem = m_ItemGenerators[i].GenerateItem();
				if(generatedItem != null)
					container.Slots[i].SetItem(generatedItem);
			}

			return container;
		}
	}
}