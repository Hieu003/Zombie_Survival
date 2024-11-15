using UnityEngine;

namespace HQFPSWeapons
{
    public class WorldManager : Singleton<WorldManager>
    {
        public Vector3 NorthDirection { get { return GetNorthDirection(); } private set { } }

        [SerializeField]
        private float m_NorthAngleDirection = 0f;


        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawRay(transform.position, GetNorthDirection());

            Gizmos.DrawWireSphere(GetNorthDirection(), 0.15f);
        }

        private Vector3 GetNorthDirection() 
        {
            return Quaternion.Euler(0, m_NorthAngleDirection, 0) * Vector3.forward;
        }
    }
}
