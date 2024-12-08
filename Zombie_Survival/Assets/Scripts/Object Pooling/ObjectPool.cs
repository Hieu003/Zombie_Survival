using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
    public class ObjectPool
    {
        public string Id { get => m_Id; }

        private GameObject m_Template;
        private Transform m_Parent;
        private string m_Id;

        private List<PoolableObject> m_AvailableObjects;
        private List<PoolableObject> m_InUseObjects;

        private int m_MinSize;
        private int m_MaxSize;
        private int m_CurrentSize;

        private bool m_Initialized;

        private float m_LastObjectGetTime;

        private bool m_AutoShrink;
        private float m_NextShrinkTime;
        private float m_AutoReleaseDelay;


        public ObjectPool(string id, GameObject template, int minSize, int maxSize, bool autoShrink, float autoReleaseDelay, Transform parent)
        {
            if(template == null)
            {
                Debug.LogError("You want to create an object pool for an object that is null!!");
                return;
            }

            // Create and store a new template, and it's hash code
            m_Template = Object.Instantiate(template, parent);
            m_Template.SetActive(false);
            m_Parent = parent;
            m_Id = id;

            // Store the min & max sizes for this pool
            m_MinSize = minSize;
            m_MaxSize = Mathf.Clamp(maxSize, m_MinSize, int.MaxValue);
            m_CurrentSize = m_MinSize;

            // Initialize the lists
            m_AvailableObjects = new List<PoolableObject>(m_MaxSize);
            m_InUseObjects = new List<PoolableObject>(m_MaxSize);
         
            // Spawn the default objects (more will be spawned if needed, according to minSize & maxSize)
            for(int i = 0;i < m_CurrentSize;i++)
            {
                PoolableObject obj = CreateNewObject(m_Template, m_Parent, m_Id);
                obj.gameObject.SetActive(false);
                m_AvailableObjects.Add(obj);
            }

            m_AutoShrink = autoShrink;
            m_AutoReleaseDelay = autoReleaseDelay;

            // Mark this pool as initialized. Means it can now be used.
            m_Initialized = true;
        }

        public void Update()
        {
            if(m_AutoShrink && m_AvailableObjects.Count > m_MinSize && Time.time > m_LastObjectGetTime + 60f && Time.time > m_NextShrinkTime)
            {
                PoolableObject objToDestroy = m_AvailableObjects[m_AvailableObjects.Count - 1];
                m_AvailableObjects.RemoveAt(m_AvailableObjects.Count - 1);

                Object.Destroy(objToDestroy);

                m_NextShrinkTime = Time.time + 0.5f;
                m_CurrentSize--;
            }
        }

        public PoolableObject GetObject()
        {
            if(!m_Initialized)
            {
                Debug.LogError("This pool can not be used, it's not initialized properly!");
                return null;
            }

            PoolableObject obj = null;

            if(m_AvailableObjects.Count > 0)
            {
                obj = m_AvailableObjects[m_AvailableObjects.Count - 1];

                // Move the object into the "in use list".
                m_AvailableObjects.RemoveAt(m_AvailableObjects.Count - 1);
                m_InUseObjects.Add(obj);
            }
            // Grow the pool.
            else if(m_CurrentSize < m_MaxSize)
            {
                m_CurrentSize++;
                obj = CreateNewObject(m_Template, m_Parent, m_Id);
                m_InUseObjects.Add(obj);
            }
            // The pool has reached it's max size, use the oldest in-use object.
            else
            {
                obj = m_InUseObjects[0];

                m_InUseObjects[0] = m_InUseObjects[m_InUseObjects.Count - 1];
                m_InUseObjects[m_InUseObjects.Count - 1] = obj;
            }

            m_LastObjectGetTime = Time.time;
            obj.gameObject.SetActive(true);
            obj.OnUse();

            if(m_AutoReleaseDelay != Mathf.Infinity)
                PoolingManager.Instance.QueueObjectRelease(obj, m_AutoReleaseDelay);

            return obj;
        }

        public bool TryPoolObject(PoolableObject obj)
        {
            if(!m_Initialized)
            {
                Debug.LogError("This pool can not be used, it's not initialized properly!");
                return false;
            }

            if(obj == null)
            {
                Debug.LogError("The object you want to pool is null!!");
                return false;
            }

            if(m_Id != obj.PoolId)
            {
                Debug.LogError("You want to put an object back in this pool, but it doesn't belong here!!");
                return false;
            }

            m_InUseObjects.Remove(obj);
            m_AvailableObjects.Add(obj);

            obj.OnReleased();
            obj.transform.SetParent(m_Parent);
            obj.gameObject.SetActive(false);

            return true;
        }

        private PoolableObject CreateNewObject(GameObject template, Transform parent, string poolId)
        {
            if(template == null)
                return null;

            GameObject obj = Object.Instantiate(template, parent);
            PoolableObject poolableObj = obj.GetComponent<PoolableObject>();

            if(poolableObj == null)
                poolableObj = obj.AddComponent<PoolableObject>();

            poolableObj.Init(poolId);

            return poolableObj;
        }
    }
}