using UnityEngine;

namespace HQFPSWeapons
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    // Sử dụng FindFirstObjectByType thay vì FindObjectOfType
                    m_Instance = Object.FindFirstObjectByType<T>();
                }

                return m_Instance;
            }
        }

        private static T m_Instance;
    }
}