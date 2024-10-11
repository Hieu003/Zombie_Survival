using UnityEditor;
using UnityEngine;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(ItemProperty.Value))]
	public class ItemPropertyDrawer : PropertyDrawer 
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position = EditorGUI.IndentedRect(position);
			position.x -= 16f
				;
			var name = property.FindPropertyRelative("m_Name");

			float initialX = position.x;

			// Source label
			position.width = 64f;
			position.height = 16f;
			UnityEngine.GUI.Label(position, "Property: ");

			// Source popup
			var allProperties = ItemDatabase.Default.GetAllPropertyNames();

			if(allProperties.Length == 0)
				return;

			position.x = position.xMax;
			position.width = 128f;

			int selectedIndex = GetStringIndex(name.stringValue, allProperties);
			selectedIndex = EditorGUI.Popup(position, selectedIndex, allProperties);
			name.stringValue = allProperties[selectedIndex];

			// Value label
			position.x = initialX;
			position.width = 64f;
			position.y = position.yMax + 4f;

			UnityEngine.GUI.Label(position, "Value: ");

			// Show the correct value based on the selected type
			position.x = position.xMax;

			DrawFloatProperty(position, property.FindPropertyRelative("m_Float"));
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 36f;
		}

		public static int GetStringIndex(string str, string[] strings)
		{
			for(int i = 0;i < strings.Length;i ++)
				if(strings[i] == str)
					return i;

			return 0;
		}

		private static void DrawFloatProperty(Rect position, SerializedProperty property)
		{
			var current = property.FindPropertyRelative("m_Current");
			var defaultVal = property.FindPropertyRelative("m_Default");

			current.floatValue = EditorGUI.FloatField(position, current.floatValue);
			defaultVal.floatValue = current.floatValue;
		}
	}
}