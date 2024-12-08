using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(ItemGenerator))]
	public class ItemGeneratorDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var method = property.FindPropertyRelative("m_Method");
			var category = property.FindPropertyRelative("m_Category");
			var item = property.FindPropertyRelative("m_Item");
			var countMin = property.FindPropertyRelative("m_CountMin");
			var countMax = property.FindPropertyRelative("m_CountMax");

			//position = EditorGUI.IndentedRect(position);
			position.x -= 4f;
			float spacing = 4f;

			EditorGUI.indentLevel -= 1;

			// Method
			position.height = 16f;
			position.y += spacing;
			EditorGUI.PropertyField(position, method);
	
			ItemGenerator.Method methodParsed = (ItemGenerator.Method)method.enumValueIndex;

			//if(methodParsed != ItemGenerator.Method.RandomItem)
			//	USEditorUtility.DoHorizontalLine(new Rect(position.x + 16f, position.yMax, position.width - 16f, 1f));

			if(methodParsed == ItemGenerator.Method.RandomItemFromCategory)
			{
				// Category
				position.y = position.yMax + spacing;
				EditorGUI.PropertyField(position, category);
			}
			else if(methodParsed == ItemGenerator.Method.CustomItem)
			{
				// Item
				position.y = position.yMax + spacing;
				EditorGUI.PropertyField(position, item);

				// Count min
				position.y = position.yMax + spacing;
				EditorGUI.PropertyField(position, countMin);

				// Count max
				position.y = position.yMax + spacing;
				EditorGUI.PropertyField(position, countMax);
			}

			EditorGUI.indentLevel += 1;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ItemGenerator.Method method = (ItemGenerator.Method)property.FindPropertyRelative("m_Method").enumValueIndex;

			float defaultHeight = 16f;
			float height = 24f;
			float spacing = 4f;

			if(method == ItemGenerator.Method.CustomItem)
				height += (defaultHeight + spacing) * 3;

			if(method == ItemGenerator.Method.RandomItemFromCategory)
				height += (defaultHeight + spacing) * 1;

			return height;
		}
	}
}