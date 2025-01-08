using System;
using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	[Serializable]
	public class ItemCategory
	{
		public string Name { get { return m_Name; } }

		public ItemData[] Items { get { return m_Items; } }

		[SerializeField]
		private string m_Name = null;

		[SerializeField]
		private ItemData[] m_Items = null;


	}
   

    [Serializable]
	public class ItemData
	{
		public string Name { get { return m_Name; } }

		public string Category { get { return m_Category; } set { m_Category = value; } }

		public Sprite Icon { get { return m_Icon; } }

		public GameObject WorldObject { get { return m_WorldObject; } }

		public int StackSize { get { return m_StackSize; } }

		public bool OnlyOneStackAllowed { get => m_OnlyOneStackAllowed; }

		public ItemDescriptionList Descriptions { get { return m_Descriptions; } }

		public ItemPropertyList Properties { get { return m_Properties; } }
       

        [SerializeField]
		private string m_Name = string.Empty;

		[SerializeField]
		[ReadOnly]
		private string m_Category = string.Empty;

		[SerializeField]
		[@Icon]
		private Sprite m_Icon = null;

		[SerializeField]
		private GameObject m_WorldObject = null;

		[SerializeField]
		[Clamp(1, 10000)]
		private int m_StackSize = 1;

		[SerializeField]
		private bool m_OnlyOneStackAllowed = false;

		[SerializeField]
		[Reorderable]
		private ItemDescriptionList m_Descriptions = null;

		[SerializeField]
		[Reorderable]
		private ItemPropertyList m_Properties = null;

       
    }
}
