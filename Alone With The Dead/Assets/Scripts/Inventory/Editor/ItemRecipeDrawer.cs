using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace HQFPSWeapons
{
//	[CustomPropertyDrawer(typeof(ItemRecipe))]
//	public class ItemRecipeDrawer : PropertyDrawer
//	{
//		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//		{
//			property.isExpanded = true;
//
//			GUI.Label(position, "Recipe", EditorStyles.boldLabel);
//
//			position.y = position.yMax + 3f;
//			EditorGUI.PropertyField(position, property.FindPropertyRelative("Duration"));
//
//			position.y = position.yMax + 3f;
//			EditorGUI.PropertyField(position, property.FindPropertyRelative("RequiredItems"));
//		}
//
//		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//		{
//			var reqItemsHeight = property.isExpanded ? new ReorderableList(property.serializedObject, property.FindPropertyRelative("RequiredItems").FindPropertyRelative("m_List")).GetHeight() : 16f;
//			return property.isExpanded ? (2 * EditorGUIUtility.singleLineHeight + reqItemsHeight) : EditorGUIUtility.singleLineHeight;
//		}
//	}
}