using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons.UserInterface
{
	[CustomEditor(typeof(Panel))]
	public class PanelEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();

			Rect rect = EditorGUILayout.GetControlRect();
			float fullWidth = rect.width;
			float btnWidth = rect.width * 0.35f;

			rect.x = fullWidth * 0.25f - btnWidth / 2;
			rect.width = btnWidth;

			var canvasGroup = serializedObject.FindProperty("m_CanvasGroup").objectReferenceValue as CanvasGroup;

			if(GUI.Button(rect, "Show") && canvasGroup != null)
			{
				canvasGroup.alpha = 1f;
				canvasGroup.blocksRaycasts = true;
			}

			rect.x = fullWidth * 0.75f - btnWidth / 2;

			if(GUI.Button(rect, "Hide") && canvasGroup != null)
			{
				canvasGroup.alpha = 0f;
				canvasGroup.blocksRaycasts = false;
			}
		}
	}
}