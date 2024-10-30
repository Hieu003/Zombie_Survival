using UnityEditor;
using UnityEngine;

namespace HQFPSWeapons
{
    [CustomPropertyDrawer(typeof(BHeader))]
    public class BHeaderDrawer : DecoratorDrawer
    {
        private const float k_HeaderSpacing = 10f;


        public override void OnGUI(Rect position)
        {
            var attr = (BHeader)attribute;

            var indentedRect = EditorGUI.IndentedRect(new Rect(position.x, position.y - (attr.IsTitle ? 5f : 4f), position.width, position.height));

            Vector2 textSize = EditorStyles.boldLabel.CalcSize(new GUIContent(attr.Name));

            Color prevColor = GUI.color;

            if (attr.IsTitle)
            {
                GUI.backgroundColor = EditorGUICustom.HighlightColor2;

                GUI.Box(new Rect(indentedRect.x, indentedRect.y + indentedRect.height * 0.5f - 3f, indentedRect.width, textSize.y * 1.2f), "");

                GUI.backgroundColor = prevColor;

                indentedRect.position += Vector2.up * indentedRect.height * 0.45f;

                GUI.Label(indentedRect, attr.Name, EditorGUICustom.CenteredBoldMiniLabel);
            }
            else 
            {
                GUI.backgroundColor = EditorGUICustom.HighlightColor1;

                Rect newRect = new Rect(indentedRect.x, indentedRect.y + indentedRect.height * 0.5f - 3f, textSize.x + 16f, textSize.y * 1.1f);

                GUI.Box(newRect, "");

                GUI.backgroundColor = prevColor;

                indentedRect.position += Vector2.up * indentedRect.height * 0.45f;

                indentedRect.x += 4f;

                GUI.Label(newRect, attr.Name, EditorGUICustom.CenteredBoldMiniLabel);
            }

            GUI.color = prevColor;
        }

        public override float GetHeight()
        {
            var attr = (BHeader)attribute;

            if (!attr.IsTitle)
                return base.GetHeight() + k_HeaderSpacing - 5;
            else
                return base.GetHeight() + k_HeaderSpacing + 5;
        }
    }
}