using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
    [CustomEditor(typeof(SurfaceManager))]
    public class SurfaceManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if(GUILayout.Button("Open Surface Editor", EditorStyles.miniButtonMid))
                SurfaceManagementWindow.Init();
        }
    }
}