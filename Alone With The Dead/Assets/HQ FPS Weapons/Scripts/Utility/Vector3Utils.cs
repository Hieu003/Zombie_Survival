using UnityEngine;

namespace HQFPSWeapons
{
    public static class Vector3Utils
    {
        public static Vector3 LocalToWorld(Vector3 vector, Transform transform)
        {
            return transform.rotation * vector;
        }

        public static Vector3 WorldToLocal(Vector3 vector, Transform transform)
        {
            return Quaternion.Inverse(transform.rotation) * vector;
        }

        public static Vector3 Convert(Vector3 vector, Transform fromSpace, Transform toSpace)
        {
            Vector3 localToWorld = LocalToWorld(vector, fromSpace);

            return WorldToLocal(localToWorld, toSpace);
        }

        public static Vector3 JitterVector(Vector3 vector, float xJit = 0, float yJit = 0, float zJit = 0)
        {
            vector = new Vector3(
                vector.x + (vector.x * Random.Range(-xJit, xJit)),
                vector.y + (vector.y * Random.Range(-yJit, yJit)),
                vector.z + (vector.z * Random.Range(-zJit, zJit))
            );

            return vector;
        }
    }
}