using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
    public class PoolingManager : Singleton<PoolingManager>
    {
        private Dictionary<string, ObjectPool> m_Pools = new Dictionary<string, ObjectPool>(50);
        private SortedList<float, PoolableObject> m_ObjectsToRelease = new SortedList<float, PoolableObject>();


        public ObjectPool CreatePool(GameObject template, int minSize, int maxSize, bool autoShrink, string poolId, float autoReleaseDelay = Mathf.Infinity)
        {
            ObjectPool pool;

            if (!m_Pools.TryGetValue(poolId, out pool))
            {
                pool = new ObjectPool(poolId, template, minSize, maxSize, autoShrink, autoReleaseDelay, transform);

                m_Pools.Add(poolId, pool);
            }

            return pool;
        }

        /// <summary>
        /// This method will use the prefab's instance id as a poolId to create a pool if one doesn't exist (it's the closest thing in ease of use to Object.Instantiate())<br></br>
        /// You can also use CreatePool() to create a custom pool for your prefabs.
        /// </summary>
        /// <returns></returns>
        public PoolableObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            PoolableObject obj = null;

            if (prefab != null)
            {
                ObjectPool pool;

                if (m_Pools.TryGetValue(prefab.GetInstanceID().ToString(), out pool))
                {
                    obj = pool.GetObject();
                }
                else
                {
                    pool = CreatePool(prefab, 10, 30, true, prefab.GetInstanceID().ToString());
                    obj = pool.GetObject();
                }
            }

            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.transform.SetParent(parent);
            }

            return obj;
        }

        /// <summary>
        /// This method will use the prefab's instance id as a poolId to create a pool if one doesn't exist (it's the closest thing in ease of use to Object.Instantiate())<br></br>
        /// You can also use CreatePool() to create a custom pool for your prefabs.
        /// </summary>
        /// <returns></returns>
        public PoolableObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            PoolableObject obj = null;

            if (prefab != null)
            {
                ObjectPool pool;

                if (m_Pools.TryGetValue(prefab.GetInstanceID().ToString(), out pool))
                {
                    obj = pool.GetObject();
                }
                else
                {
                    pool = CreatePool(prefab, 10, 30, true, prefab.GetInstanceID().ToString());
                    obj = pool.GetObject();
                }
            }

            if (obj != null)
                obj.transform.SetPositionAndRotation(position, rotation);

            return obj;
        }

        public PoolableObject GetObject(string poolId, Vector3 position, Quaternion rotation, Transform parent)
        {
            PoolableObject obj = GetObject(poolId);

            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.transform.SetParent(parent);
            }

            return obj;
        }

        public PoolableObject GetObject(string poolId, Vector3 position, Quaternion rotation)
        {
            PoolableObject obj = GetObject(poolId);

            if (obj != null)
                obj.transform.SetPositionAndRotation(position, rotation);

            return obj;
        }

        public PoolableObject GetObject(string poolId)
        {
            ObjectPool pool = null;
            m_Pools.TryGetValue(poolId, out pool);

            if(pool != null)
                return pool.GetObject();
            else
                return null;
        }

        public bool ReleaseObject(PoolableObject obj)
        {
            if(obj == null)
                return false;

            ObjectPool pool = null;

            if(!m_Pools.ContainsKey(obj.PoolId))
                print("key not found: " + obj.PoolId);

            m_Pools.TryGetValue(obj.PoolId, out pool);

            if(pool != null)
                return pool.TryPoolObject(obj);
            else
                return false;
        }

        public void QueueObjectRelease(PoolableObject obj, float delay)
        {
            float key = Time.time + delay;

            if(m_ObjectsToRelease.ContainsKey(key))
                key += Random.Range(0.05f, 0.5f);

            m_ObjectsToRelease.Add(key, obj);
        }

        private void Update()
        {
            if(m_ObjectsToRelease.Count > 0 && Time.time > m_ObjectsToRelease.Keys[0])
            {
                ReleaseObject(m_ObjectsToRelease.Values[0]);
                m_ObjectsToRelease.RemoveAt(0);
            }
        }
    }
}