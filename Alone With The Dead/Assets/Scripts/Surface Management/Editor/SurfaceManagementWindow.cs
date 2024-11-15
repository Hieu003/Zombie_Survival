using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
    public class SurfaceManagementWindow : EditorWindow
    {
        private SurfaceInfo[] m_Surfaces = null;
        private string[] m_SurfaceNames = null;
        private int m_SelectedSurface = 0;
        private Vector2 m_ScrollPos = Vector2.zero;

        private UnityEditor.Editor m_CachedSurfaceInspector;


        [MenuItem("Tools/HQ FPS Weapons Pack/Surfaces...", false, 7)]
        public static void Init()
        {
            GetWindow<SurfaceManagementWindow>(true, "Surfaces");
        }

        private void OnEnable()
        {
            LoadSurfaceAssets();
         
            if(m_Surfaces != null)
                CreateInspectorForSelectedSurface();
        }

        private void OnProjectChange()
        {
            LoadSurfaceAssets();
        }

        private void OnGUI()
        {
            if(m_Surfaces != null)
            {
                int previousSelectedSurf = m_SelectedSurface;
                m_SelectedSurface = GUILayout.Toolbar(m_SelectedSurface, m_SurfaceNames);

                if(m_SelectedSurface != previousSelectedSurf)
                    CreateInspectorForSelectedSurface();

                // GUI
                EditorGUILayout.Space();

                m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(m_SurfaceNames[m_SelectedSurface], EditorGUICustom.TitleLabel);

                m_CachedSurfaceInspector.DrawDefaultInspector();

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();
            }
        }

        private void LoadSurfaceAssets()
        {
            m_Surfaces = null;
            m_SurfaceNames = null;

            string[] surfGUIDs = AssetDatabase.FindAssets("t:SurfaceInfo");

            if(surfGUIDs.Length > 0)
            {
                m_Surfaces = new SurfaceInfo[surfGUIDs.Length];
                m_SurfaceNames = new string[surfGUIDs.Length];

                for(int i = 0;i < surfGUIDs.Length;i++)
                {
                    m_Surfaces[i] = AssetDatabase.LoadAssetAtPath<SurfaceInfo>(AssetDatabase.GUIDToAssetPath(surfGUIDs[i]));
                    m_SurfaceNames[i] = m_Surfaces[i].name.DoUnityLikeNameFormat();
                }
            }
        }

        private void CreateInspectorForSelectedSurface()
        {
            UnityEditor.Editor.CreateCachedEditor(m_Surfaces[m_SelectedSurface], null, ref m_CachedSurfaceInspector);
        }
    }
}