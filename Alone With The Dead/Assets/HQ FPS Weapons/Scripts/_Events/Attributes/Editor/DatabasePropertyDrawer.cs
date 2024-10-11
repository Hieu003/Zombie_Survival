using UnityEngine;
using UnityEditor;
using System.Linq;

namespace HQFPSWeapons.Inventory
{
	[CustomPropertyDrawer(typeof(DatabaseProperty))]
	public class DatabasePropertyDrawer : PropertyDrawer
	{
		private static string[] m_AllProperties;


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
		{
			if(property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.HelpBox(EditorGUI.IndentedRect(position), "The ItemProperty attribute runs just on strings.", MessageType.Error);
				return;
			}

			if(m_AllProperties == null && ItemDatabase.Default != null)
				m_AllProperties = ItemDatabase.Default.GetAllPropertyNames();

			if(m_AllProperties != null)
				property.stringValue = IndexToString(EditorGUI.Popup(position, label.text, StringToIndex(property.stringValue), m_AllProperties));
		}

		private int StringToIndex(string s)
		{
			for(int i = 0;i < m_AllProperties.Length;i ++)
			{
				if(m_AllProperties[i] == s)
					return i;
			}

			return 0;
		}

		private string IndexToString(int i)
		{
			return m_AllProperties.Length > i ? m_AllProperties[i] : "";
		}
	}
}