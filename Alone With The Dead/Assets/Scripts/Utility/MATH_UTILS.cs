using UnityEngine;

namespace HQFPSWeapons
{
	public class MATH_UTILS 
	{
		public static Vector3[] GetArchVertices(Vector3 startDirection, Vector3 endDirection, float radius, int vertexCount, Transform relativeTo = null)
		{
			startDirection.Normalize();
			endDirection.Normalize();

			vertexCount = Mathf.Clamp(vertexCount, 3, 100);
			Vector3[] vertices = new Vector3[vertexCount];
			float deltaRadians = Vector3.Angle(startDirection, endDirection) * Mathf.Deg2Rad / vertexCount;

			for(int i = 0;i < vertexCount;i ++)
			{
				vertices[i] = Vector3.RotateTowards(startDirection, endDirection, deltaRadians * i, 0f);
				if(relativeTo != null)
					vertices[i] = relativeTo.TransformVector(vertices[i]);
			}

			return vertices;
		}

		public static float GetBezier(float time)
		{
			time = Mathf.Clamp01(time);
			return (time * time) * (3f - 2 * time);
		}

		public static Vector3 GetNaNSafeVector3(Vector3 vector3)
		{
			if(float.IsNaN(vector3.x))
				vector3.x = 0f;

			if(float.IsNaN(vector3.y))
				vector3.y = 0f;

			if(float.IsNaN(vector3.z))
				vector3.z = 0f;

			return vector3;
		}

		private MATH_UTILS() {}
	}
}
