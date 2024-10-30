using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	public abstract class CopyPasteBase<T> : PropertyDrawer
	{
		private static T m_Clipboard;
		private static SerializedProperty m_PopupTarget = null;


		protected static void DoCopy()
		{
			if(m_PopupTarget != null)
			{
				var sourceValue = m_PopupTarget.GetValue<T>();

				if(sourceValue as CloneableObject<T> != null)
					m_Clipboard = (sourceValue as CloneableObject<T>).GetMemberwiseClone();
				else if(typeof(T).IsValueType)
					m_Clipboard = sourceValue;
				else
					Debug.LogError("Copy & paste is not available for objects of type " + typeof(T).Name + ". The type should either be a value type, or reference type deriving from CloneableObject.");
			}
		}
			
		protected static void DoPaste()
		{
			if(m_Clipboard != null && m_PopupTarget != null)
			{
				m_PopupTarget.serializedObject.Update();
				Undo.RegisterCompleteObjectUndo(m_PopupTarget.serializedObject.targetObject, typeof(T).Name + " Paste");
				m_PopupTarget.SetValue<T>(m_Clipboard);
				EditorUtility.SetDirty(m_PopupTarget.serializedObject.targetObject);
				m_PopupTarget.serializedObject.ApplyModifiedProperties();

				if(m_Clipboard as CloneableObject<T> != null)
					m_Clipboard = (m_Clipboard as CloneableObject<T>).GetMemberwiseClone();
			}
		}

		public void OnGUI(Rect rect, SerializedProperty property, GUIContent label, string menuName) 
		{
			var current = Event.current;
			if(current.type == EventType.ContextClick) 
			{
				var mousePos = current.mousePosition;
				if(new Rect(rect.x, rect.y, rect.width, 16f).Contains(mousePos))
				{
					property.serializedObject.Update();
					m_PopupTarget = property.Copy();
					EditorUtility.DisplayPopupMenu(new Rect(mousePos.x,mousePos.y, 64, 16), "CONTEXT/" + menuName + "/", null);
				}
			}
			else
			{
				label = EditorGUI.BeginProperty(rect, label, property);
				EditorGUI.PropertyField(rect, property, true);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, true);
		}
	}
}
