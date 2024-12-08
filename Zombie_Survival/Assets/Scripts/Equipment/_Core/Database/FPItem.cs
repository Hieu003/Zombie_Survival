using System;
using UnityEngine;

namespace HQFPSWeapons
{
    [Serializable]
    public class FPItem
    {
        #region Internal
        /// <summary>
        /// <para>How will this First Person item be instantiated in the world</para>
        /// <para>"ReplaceCurrentItem": Replace the "Skinned Mesh Renderer" mesh</para>
        /// <para>"SpawnUnderRoot": Spawn under the root object (e.g. spawn under the weapon bone)</para>
        /// </summary>
        public enum InstantiateMethod 
        {
            ReplaceCurrentItem,
            SpawnUnderRoot
        } 

        [Serializable]
        public class EquipmentSkin
        {
            public SkinnedMeshRenderer FPMesh = null;

            public Material[] FPMaterials = null;
        }
        #endregion

        //Properties
        public string Name => m_Name;
        public GameObject FPItemData => m_FPItemData;
        public InstantiateMethod SpawnMethod => m_SpawnMethod;

        [DatabaseItem]
        [SerializeField]
        private string m_Name = string.Empty;

        [SerializeField]
        private InstantiateMethod m_SpawnMethod = InstantiateMethod.ReplaceCurrentItem;

        [Space]

        [SerializeField]
        private GameObject m_FPItemData = null;

        [SerializeField]
        private EquipmentSkin[] m_EquipmentSkins = null;


        public EquipmentSkin GetEquipmentSkin(int index) 
        {
            if (m_EquipmentSkins.Length == 0)
                return null;
            else
                return m_EquipmentSkins[Mathf.Clamp(index, 0, m_EquipmentSkins.Length - 1)];
        }
    }
}
