using System;

namespace HQFPSWeapons
{
	[Serializable]
	public struct EasingOptions
	{
		public Easings.Function Function;

		public float Duration;
	}
}