using UnityEngine;

namespace HQFPSWeapons
{
    public class PlayerSpawnPoints : MonoBehaviour
    {
        [SerializeField]
        private float m_MaxVerticalRaycast = 2f;

        [SerializeField]
        private LayerMask m_GroundLayerMask = 0;

        private Transform[] m_Spawnpoints;


        public Vector3 GetRandomSpawnPoint()
        {
            if (m_Spawnpoints != null)
            {
                Vector3 spawnPoint = m_Spawnpoints[Random.Range(0, m_Spawnpoints.Length)].position;

                RaycastHit hit;

                if (Physics.Raycast(spawnPoint, -transform.up, out hit, m_MaxVerticalRaycast, m_GroundLayerMask))
                {
                    spawnPoint = hit.point + (Vector3.up * 0.1f);
                }

                return spawnPoint;
            }
            else return Vector3.zero;
        }

        public Quaternion GetRandomRotation()
        {
            return Quaternion.Euler(0, Random.Range(0, 360), 0);
        }

        private void Awake()
        {
            m_Spawnpoints = GetComponentsInChildren<Transform>();
        }
    }
}
