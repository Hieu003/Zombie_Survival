using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(DelayedSound))]
	public class DelayedClipDrawer : PropertyDrawer
	{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.x -= 6f;

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}