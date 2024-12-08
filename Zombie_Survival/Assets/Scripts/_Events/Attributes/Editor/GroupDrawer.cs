using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
    [CustomPropertyDrawer(typeof(GroupAttribute))]
    public class GroupDrawer : PropertyDrawer
    {
        private const int k_HeaderPadding = 8;
        private const int k_HorizontalPadding = 16;
        private const int k_VerticalPadding = 8;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Color normalGUIColor = GUI.color;
            Color normalContentColor = GUI.contentColor;

            if(property.isExpanded)
                GUI.Box(position, "");

            Rect rect = new Rect(position.x, position.y, position.width, 24);

            GUIStyle buttonStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).button;

            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.padding = new RectOffset(k_HeaderPadding, 0, 0, 0);

            if(!EditorGUIUtility.isProSkin)
                GUI.contentColor = new Color(0.3f, 0.3f, 0.3f, 1f);

            GUI.color = property.isExpanded ? normalGUIColor : new Color(normalGUIColor.r, normalGUIColor.g, normalGUIColor.b, 0.6f);

            if(GUI.Button(EditorGUI.IndentedRect(rect), property.displayName, buttonStyle))
                property.isExpanded = !property.isExpanded;

            GUI.contentColor = normalContentColor;
            GUI.color = normalGUIColor;
           
            rect = new Rect(rect.x + k_HorizontalPadding, rect.y + k_VerticalPadding, rect.width - k_HorizontalPadding * 2, EditorGUIUtility.singleLineHeight);

            rect.y = rect.yMax + EditorGUIUtility.standardVerticalSpacing;

            normalGUIColor = GUI.color;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.825f);

            if(property.isExpanded)
            {
                foreach(SerializedProperty child in property.GetChildren())
                {
                    EditorGUI.PropertyField(rect, child, true);
                    rect.y = rect.y + EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            GUI.color = normalGUIColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true) + k_HeaderPadding + (property.isExpanded ? k_VerticalPadding : 0);
        }
    }
}