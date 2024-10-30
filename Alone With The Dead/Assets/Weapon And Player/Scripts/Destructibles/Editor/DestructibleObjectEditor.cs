using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomEditor(typeof(DestructibleObject))]
	public class DestructibleObjectEditor : UnityEditor.Editor
	{
		private Transform m_AutoSearchRoot;


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		
			USEditorUtility.DoHorizontalLine();

			m_AutoSearchRoot = (Transform)EditorGUILayout.ObjectField("Search Root", m_AutoSearchRoot, typeof(Transform), true);

			if(GUILayout.Button("Search For Fragments") && m_AutoSearchRoot != null)
			{
				var dynamicParts = new List<DestructibleObject.DebrisFragment>();

				foreach(Transform child in m_AutoSearchRoot)
				{
					var rigidbody = child.GetComponent<Rigidbody>();
					if(rigidbody != null)
						dynamicParts.Add(new DestructibleObject.DebrisFragment(rigidbody));

					serializedObject.Update();
					(serializedObject.targetObject as DestructibleObject).SetDebrisFragments (dynamicParts);
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}