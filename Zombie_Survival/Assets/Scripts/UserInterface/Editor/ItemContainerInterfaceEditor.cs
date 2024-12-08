using UnityEngine;
using UnityEditor;
using HQFPSWeapons.UserInterface;

namespace HQFPSWeapons
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ItemContainerInterface))]
	public class ItemContainerInterfaceEditor : UnityEditor.Editor
	{
		private SerializedProperty m_SlotTemplate;
		private SerializedProperty m_SlotsParent;


		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			base.OnInspectorGUI();

			if(!Application.isPlaying)
			{
				EditorGUILayout.Space();

				if(!serializedObject.isEditingMultipleObjects && GUILayout.Button("Spawn Default Slots"))
					(serializedObject.targetObject as ItemContainerInterface).GenerateSlots();
			}

			if(!m_SlotTemplate.objectReferenceValue || !m_SlotsParent.objectReferenceValue)
				EditorGUILayout.HelpBox("Make sure a slot template and parent are assigned!", MessageType.Error);

			serializedObject.ApplyModifiedProperties();
		}

		private void OnEnable()
		{
			m_SlotTemplate = serializedObject.FindProperty("m_SlotTemplate");
			m_SlotsParent = serializedObject.FindProperty("m_SlotsParent");
		}
	}
}
