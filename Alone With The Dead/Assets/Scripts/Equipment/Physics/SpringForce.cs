using System;
using UnityEngine;

namespace HQFPSWeapons
{
	[Serializable]
	public struct SpringForce
	{
		public SpringForce Default { get { return new SpringForce() { Force = Vector3.zero, Distribution = 1 }; } }

		public Vector3 Force;

		[Range(1, 20)]
		public int Distribution;
	}
}