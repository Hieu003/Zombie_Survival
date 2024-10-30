using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
    [CustomPropertyDrawer(typeof(ShowIf))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			if(CanShow(property))
			{
				var indentedRect = position;
				float indentation = ((ShowIf)attribute).m_Indentation;

				position.x += indentation;
				position.width -= indentation;
				EditorGUI.PropertyField(position, property, label, true);
			}
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
			if (!CanShow(property))
				// HACK. if we don't use this, extra spacing height will be added by Unity.
				return -EditorGUIUtility.standardVerticalSpacing;
            else
				return EditorGUI.GetPropertyHeight(property, true);
        }

		private bool CanShow(SerializedProperty property)
		{
			var attr = (ShowIf)attribute;

			string parentPath = string.Empty;
			parentPath = property.GetParentPath();
			SerializedProperty conditionProperty = property.serializedObject.FindProperty(parentPath + (parentPath != string.Empty ? "." : "") + attr.m_PropertyName);

			if(conditionProperty != null)
			{
				if(conditionProperty.propertyType == SerializedPropertyType.Boolean)
					return attr.m_RequiredBool == conditionProperty.boolValue;
				else if(conditionProperty.propertyType == SerializedPropertyType.ObjectReference)
					return attr.m_RequiredBool == (conditionProperty.objectReferenceValue != null);
				else if(conditionProperty.propertyType == SerializedPropertyType.Integer)
					return attr.m_RequiredInt == conditionProperty.intValue;
				else if(conditionProperty.propertyType == SerializedPropertyType.Enum)
					return attr.m_RequiredInt == conditionProperty.intValue;
				else if(conditionProperty.propertyType == SerializedPropertyType.Float)
					return attr.m_RequiredFloat == conditionProperty.floatValue;
				else if(conditionProperty.propertyType == SerializedPropertyType.String)
					return attr.m_RequiredString == conditionProperty.stringValue;
				else if(conditionProperty.propertyType == SerializedPropertyType.Vector3)
					return attr.m_RequiredVector3 == conditionProperty.vector3Value;
			}

			return false;
		}
    }
}