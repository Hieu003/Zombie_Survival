using System;
using UnityEngine;

namespace HQFPSWeapons
{
    public class GunSettings
    {
		[Serializable]
		public class Shooting
		{
			[BHeader("General Settings")]

			[Tooltip("The layers that will be affected when you fire.")]
			public LayerMask Mask;

			[Tooltip("If something is farther than this distance threeshold, it will not be affected by the shot.")]
			public float MaxDistance = 150f;

			[Range(1, 20)]
			[Tooltip("The amount of rays that will be sent in the world " +
				"(basically the amount of projectiles / bullets that will be fired at a time).")]
			public int RayCount = 1;

			public HitscanImpact RayImpact;

			[Range(0f, 2f)]
			public float AimThreeshold = 0.35f;

			[BHeader("Ray Spread")]

			[Tooltip("How the bullet spread will transform (in continuous use) on the duration of the magazine, the max x value(1) will be used if the whole magazine has been used")]
			public AnimationCurve SpreadOverTime = new AnimationCurve(
				new Keyframe(0f, .8f),
				new Keyframe(1f, 1f));

			[Range(0f, 3f)]
			public float AimSpreadFactor = 0.8f;

			[Range(0f, 3f)]
			public float CrouchSpreadFactor = 0.95f;

			[Range(0f, 3f)]
			public float WalkSpreadFactor = 0.95f;

			[Range(0f, 3f)]
			public float RunSpreadFactor = 0.95f;

			[Range(0f, 3f)]
			public float JumpSpreadFactor = 1.5f;
		}

		[Serializable]
		public class HitscanImpact
		{
			[Range(0f, 1000f)]
			[SerializeField]
			[Tooltip("The damage at close range.")]
			private float m_MaxDamage = 15f;

			[Range(0f, 1000f)]
			[SerializeField]
			[Tooltip("The impact impulse that will be transfered to the rigidbodies at contact.")]
			private float m_MaxImpulse = 15f;

			[SerializeField]
			[Tooltip("How damage and impulse lowers over distance.")]
			private AnimationCurve m_DistanceCurve = new AnimationCurve(
				new Keyframe(0f, 1f),
				new Keyframe(0.8f, 0.5f),
				new Keyframe(1f, 0f));

			/// <summary>
			/// 
			/// </summary>
			/// <param name="distance"></param>
			/// <param name="maxDistance"></param>
			public float GetDamageAtDistance(float distance, float maxDistance)
			{
				return ApplyCurveToValue(m_MaxDamage, distance, maxDistance);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns>The impulse at distance.</returns>
			/// <param name="distance">Distance.</param>
			/// <param name="maxDistance">Max distance.</param>
			public float GetImpulseAtDistance(float distance, float maxDistance)
			{
				return ApplyCurveToValue(m_MaxImpulse, distance, maxDistance);
			}

			private float ApplyCurveToValue(float value, float distance, float maxDistance)
			{
				float maxDistanceAbsolute = Mathf.Abs(maxDistance);
				float distanceClamped = Mathf.Clamp(distance, 0f, maxDistanceAbsolute);

				return value * m_DistanceCurve.Evaluate(distanceClamped / maxDistanceAbsolute);
			}
		}
	}
}