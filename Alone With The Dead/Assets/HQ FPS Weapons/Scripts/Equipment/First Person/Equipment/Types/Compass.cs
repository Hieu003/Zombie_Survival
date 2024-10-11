using UnityEngine;

namespace HQFPSWeapons
{
    public class Compass : EquipmentItem
    {
        [BHeader("Compass")]

        [SerializeField]
        private string m_CompassRoseBoneName;

        [SerializeField]
        private Vector3 m_CompassRoseRotationAxis;

        private Transform m_CompassRose;

        private Vector3 m_CurrentRoseRotation;
        private Vector3 m_NorthDirection;


        private void Start()
        {
            m_CompassRose = m_EHandler.EquipSettings.Armature.FindDeepChild(m_CompassRoseBoneName);

            if (WorldManager.Instance != null)
                m_NorthDirection = WorldManager.Instance.NorthDirection;
            else
                m_NorthDirection = Vector3.forward;
        }

        private void LateUpdate()
        {
            float angle = -Vector3.SignedAngle(Player.transform.forward, m_NorthDirection, Vector3.up);

            m_CurrentRoseRotation = Vector3.Scale(new Vector3(angle, angle, angle), m_CompassRoseRotationAxis);

            m_CompassRose.localRotation = Quaternion.Euler(m_CompassRose.localEulerAngles + m_CurrentRoseRotation);
        }
    }
}
