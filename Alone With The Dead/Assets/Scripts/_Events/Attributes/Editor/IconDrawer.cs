using System;
using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(Icon))]
	public class IconDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position = EditorGUI.IndentedRect(position);

			var attr = attribute as Icon;

			position.height = EditorGUIUtility.singleLineHeight;
			GUI.Label(position, label);
		
			position.xMin += EditorGUIUtility.labelWidth;
			position.height = attr.Size;
			position.width = attr.Size;

			property.objectReferenceValue = EditorGUI.ObjectField(position, property.objectReferenceValue, typeof(Sprite), false);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return (attribute as Icon).Size;
		}
	}
}