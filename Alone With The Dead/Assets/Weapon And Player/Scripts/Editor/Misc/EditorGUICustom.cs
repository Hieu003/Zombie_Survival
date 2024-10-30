using System;
using UnityEditor;
using UnityEngine;

namespace HQFPSWeapons
{
    public static class EditorGUICustom
    {
        public static event Action OneSecondPassed;

        private static double m_OneSecondTimer;

        public static GUIStyle TitleLabel { get; private set; }
        public static GUIStyle CenteredMiniLabel { get; private set; }
        public static GUIStyle CenteredBoldMiniLabel { get; private set; }
        public static GUIStyle MiniGreyLabel { get; private set; }
        public static GUIStyle BoldMiniGreyLabel { get; private set; }
        public static Color HighlightColor1 { get; private set; }
        public static Color HighlightColor2 { get; private set; }

        public static readonly Color SeparatorColor = EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f) : new Color(0.5f, 0.5f, 0.5f);

        private static GUIStyle m_SeparatorStyle;

        
        static EditorGUICustom()
        {
            EditorApplication.update += OnEditorUpdate;
           
            m_SeparatorStyle = new GUIStyle();
            m_SeparatorStyle.normal.background = EditorGUIUtility.whiteTexture;
            m_SeparatorStyle.stretchWidth = true;
            m_SeparatorStyle.margin = new RectOffset(0, 0, 7, 7);

            TitleLabel = new GUIStyle(EditorStyles.boldLabel);
            TitleLabel.fontSize = 12;
            TitleLabel.normal.textColor = EditorGUIUtility.isProSkin ? new Color(1,1,1,0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
            TitleLabel.alignment = TextAnchor.UpperCenter;

            CenteredMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            CenteredMiniLabel.normal.textColor = new Color(1, 1, 1, 0.8f);
            CenteredMiniLabel.fontSize = 11;
            CenteredMiniLabel.alignment = TextAnchor.UpperCenter;

            CenteredBoldMiniLabel = new GUIStyle(CenteredMiniLabel);
            CenteredBoldMiniLabel.fontStyle = FontStyle.Bold;

            MiniGreyLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            MiniGreyLabel.alignment = TextAnchor.MiddleLeft;

            BoldMiniGreyLabel = new GUIStyle(MiniGreyLabel);
            BoldMiniGreyLabel.fontStyle = FontStyle.Bold;

            HighlightColor1 = new Color(0.65f, 0.7f, 0.8f, 1f);
            HighlightColor2 = new Color(0.5f, 0.5f, 0.55f, 1f);
        }

        private static void OnEditorUpdate()
        {
            if(EditorApplication.timeSinceStartup > m_OneSecondTimer + 1f)
            {
                if(OneSecondPassed != null)
                    OneSecondPassed();

                m_OneSecondTimer = EditorApplication.timeSinceStartup;
            }
        }

        public static void Separator(Color rgb, float thickness = 1)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, m_SeparatorStyle, GUILayout.Height(thickness));

            if(Event.current.type == EventType.Repaint)
            {
                Color restoreColor = GUI.color;
                GUI.color = rgb;
                m_SeparatorStyle.Draw(position, false, false, false, false);
                GUI.color = restoreColor;
            }
        }

        public static void Separator(float thickness, GUIStyle splitterStyle)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitterStyle, GUILayout.Height(thickness));

            if(Event.current.type == EventType.Repaint)
            {
                Color restoreColor = GUI.color;
                GUI.color = SeparatorColor;
                splitterStyle.Draw(position, false, false, false, false);
                GUI.color = restoreColor;
            }
        }

        public static void Separator(float thickness = 1)
        {
            Separator(thickness, m_SeparatorStyle);
        }

        public static void Separator(Rect position, Color color)
        {
            if(Event.current.type == EventType.Repaint)
            {
                Color restoreColor = GUI.color;
                GUI.color = color;
                m_SeparatorStyle.Draw(position, false, false, false, false);
                GUI.color = restoreColor;
            }
        }

        public static void Separator(Rect position)
        {
            Separator(position, SeparatorColor);
        }

        public static void EnumPopupNonAlloc(Rect rect, SerializedProperty property, ref string[] names)
        {
            property.enumValueIndex = EditorGUI.Popup(rect, property.enumValueIndex, names);
        }

        public static int IndexOfString(string str, string[] allStrings)
        {
            for(int i = 0;i < allStrings.Length;i++)
            {
                if(allStrings[i] == str)
                    return i;
            }

            return 0;
        }

        public static string StringAtIndex(int i, string[] allStrings)
        {
            return allStrings.Length > i ? allStrings[i] : "";
        }
    }
}