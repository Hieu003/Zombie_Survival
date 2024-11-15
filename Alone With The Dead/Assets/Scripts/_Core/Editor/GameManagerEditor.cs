using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomEditor(typeof(GameManager))]
	public class GameManagerEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUILayout.Space();

			if(GUILayout.Button("Get All Materials"))
			{
				var allMaterials = AssetDatabase.FindAssets("t:Material");
				var materialsList = new List<Material>();
			
				foreach(var guid in allMaterials)
				{
					var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));

					if(material != null)
						materialsList.Add(material);
				}

				(serializedObject.targetObject as GameManager).PreloadedMaterials = materialsList.ToArray();
			}

			EditorGUILayout.Space();
				
			serializedObject.ApplyModifiedProperties();
		}
	}
}