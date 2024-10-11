using UnityEngine;

namespace HQFPSWeapons
{
	public static class ShakeManager
	{
		public static Message<ShakeEventData> ShakeEvent = new Message<ShakeEventData>();
	}

	public struct ShakeEventData
	{
		public Vector3 Position { get; private set; }
		public float Radius { get; private set; }
		public float Scale { get; private set; }
		public ShakeType ShakeType { get; private set; }


		public ShakeEventData(Vector3 position, float radius, float scale, ShakeType shakeType)
		{
			Position = position;
			Radius = radius;
			Scale = scale;
			ShakeType = shakeType;
		}
	}

	public enum ShakeType
	{
		Explosion
	}
}