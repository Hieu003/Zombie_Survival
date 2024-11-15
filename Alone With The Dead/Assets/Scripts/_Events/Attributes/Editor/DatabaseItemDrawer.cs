using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons.Inventory
{
	[CustomPropertyDrawer(typeof(DatabaseItem))]
	public class DatabaseItemDrawer : PropertyDrawer 
	{
		private string[] m_AllItems;
		private string[] m_AllItemsFull;


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
		{
			if(property.propertyType != SerializedPropertyType.String)
				EditorGUI.HelpBox(position, "The Item attribute runs just on strings.", MessageType.Error);

			if(m_AllItems == null && ItemDatabase.Default != null)
			{
				m_AllItems = ItemDatabase.Default.GetAllItemNames().ToArray();
				m_AllItemsFull = ItemDatabase.Default.GetAllItemNamesFull().ToArray();
			}

			if(m_AllItems != null)
				property.stringValue = IndexToString(EditorGUI.Popup(position, label.text, StringToIndex(property.stringValue), m_AllItemsFull));
		}

		private int StringToIndex(string s)
		{
			for(int i = 0;i < m_AllItems.Length;i ++)
			{
				if(m_AllItems[i] == s)
					return i;
			}
				
			return 0;
		}

		private string IndexToString(int i)
		{
			return m_AllItems.Length > i ? m_AllItems[i] : "";
		}
	}
}