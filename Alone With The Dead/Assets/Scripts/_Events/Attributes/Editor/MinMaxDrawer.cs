using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minMaxAttribute = (MinMaxAttribute)attribute;
            var propertyType = property.propertyType;

            label.tooltip = minMaxAttribute.min.ToString("F2") + " to " + minMaxAttribute.max.ToString("F2");

            Rect ctrlRect = EditorGUI.PrefixLabel(position, label);
            Rect[] splittedRect = SplitRect(ctrlRect, 3);

            if (propertyType == SerializedPropertyType.Vector2)
            {
                EditorGUI.BeginChangeCheck();

                Vector2 vector = property.vector2Value;
                float minVal = vector.x;
                float maxVal = vector.y;

                minVal = EditorGUI.FloatField(splittedRect[0], float.Parse(minVal.ToString("F2")));
                maxVal = EditorGUI.FloatField(splittedRect[2], float.Parse(maxVal.ToString("F2")));

                EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal,
                minMaxAttribute.min, minMaxAttribute.max);

                if (minVal < minMaxAttribute.min)
                {
                    minVal = minMaxAttribute.min;
                }

                if (maxVal > minMaxAttribute.max)
                {
                    maxVal = minMaxAttribute.max;
                }

                vector = new Vector2(minVal > maxVal ? maxVal : minVal, maxVal);

                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2Value = vector;
                }

            }
            else if (propertyType == SerializedPropertyType.Vector2Int)
            {
                EditorGUI.BeginChangeCheck();

                Vector2Int vector = property.vector2IntValue;
                float minVal = vector.x;
                float maxVal = vector.y;

                minVal = EditorGUI.FloatField(splittedRect[0], minVal);
                maxVal = EditorGUI.FloatField(splittedRect[2], maxVal);

                EditorGUI.MinMaxSlider(splittedRect[1], ref minVal, ref maxVal,
                minMaxAttribute.min, minMaxAttribute.max);

                if (minVal < minMaxAttribute.min)
                {
                    maxVal = minMaxAttribute.min;
                }

                if (minVal > minMaxAttribute.max)
                {
                    maxVal = minMaxAttribute.max;
                }

                vector = new Vector2Int(Mathf.FloorToInt(minVal > maxVal ? maxVal : minVal), Mathf.FloorToInt(maxVal));

                if (EditorGUI.EndChangeCheck())
                {
                    property.vector2IntValue = vector;
                }
            }
        }

        Rect[] SplitRect(Rect rectToSplit, int n)
        {
            Rect[] rects = new Rect[n];

            float indentLevel = EditorGUI.indentLevel;

            if (indentLevel > 1)
                indentLevel += 15f;
            else
                indentLevel = 1;

            for (int i = 0; i < n; i++)
            {
                rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n) - 40f, rectToSplit.position.y, rectToSplit.width / n + indentLevel, rectToSplit.height);
            }

            int padding = (int)rects[0].width - 35;

            if (indentLevel > 1)
                padding -= 20;

            rects[0].x += 15f;
            rects[0].width -= padding - 10f;
            rects[2].width -= padding - 10f;

            rects[1].x -= padding - 20f;
            rects[1].width += padding * 2f;

            rects[2].x += padding + 20f;

            return rects;

        }
    }
}