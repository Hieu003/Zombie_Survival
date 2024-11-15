using UnityEngine;
using System;

namespace HQFPSWeapons
{
    /// <summary>
    /// Represents an asset that stores all of the first person arms
    /// </summary>
    [CreateAssetMenu(menuName = "HQ FPS Weapons Pack/First Person Arms")]
    public class FirstPersonArms : ScriptableObject
    {
        public static FirstPersonArms Default
        {
            get
            {
                if (m_Default == null)
                {
                    var allDatabases = Resources.LoadAll<FirstPersonArms>("");
                    if (allDatabases != null && allDatabases.Length > 0)
                        m_Default = allDatabases[0];
                }

                return m_Default;
            }
        }

        [SerializeField]
        [Group]
        private FirstPersonArmsData[] m_FirstPersonArms = null;

        private static FirstPersonArms m_Default;

        public FirstPersonArmsData GetFirstPersonArms(int index) 
        {
            return m_FirstPersonArms[Mathf.Clamp(index, 0, m_FirstPersonArms.Length)];
        }

        public int GetFirstPersonArmsCount() 
        {
            return m_FirstPersonArms.Length;
        }
    }

    [Serializable]
    public class FirstPersonArmsData
    {
        public SkinnedMeshRenderer LeftArm;
        public SkinnedMeshRenderer RightArm;
        public GameObject AccesoryModel;
    }
}
