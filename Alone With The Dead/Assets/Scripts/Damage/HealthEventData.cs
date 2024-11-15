using UnityEngine;

namespace HQFPSWeapons
{
	public enum DamageType
	{
		Generic,
		Cut,
		Hit,
		Stab,
		Bullet
	}

	/// <summary>
	/// 
	/// </summary>
	public struct HealthEventData
	{
		/// <summary> </summary>
		public float Delta { get; set; }

		/// <summary> </summary>
		public LivingEntity Source { get; private set; }

		public DamageType DamageType { get; private set; }

		/// <summary> </summary>
		public Vector3 HitPoint { get; private set; }

		/// <summary> </summary>
		public Vector3 HitDirection { get; private set; }

		/// <summary> </summary>
		public float HitImpulse { get; private set; }

		/// <summary> </summary>
		public Vector3 HitNormal { get; private set; }


		public HealthEventData(float delta, LivingEntity source = null)
		{
			Delta = delta;

			DamageType = DamageType.Generic;

			HitPoint = Vector3.zero;
			HitDirection = Vector3.zero;
			HitImpulse = 0f;

			HitNormal = Vector3.zero;

			Source = source;
		}

		public HealthEventData(float delta, DamageType damageType, LivingEntity source = null)
		{
			Delta = delta;

			DamageType = damageType;

			HitPoint = Vector3.zero;
			HitDirection = Vector3.zero;
			HitImpulse = 0f;

			HitNormal = Vector3.zero;

			Source = source;
		}

		public HealthEventData(float delta, Vector3 hitPoint, Vector3 hitDirection, float hitImpulse, LivingEntity source = null)
		{
			Delta = delta;

			DamageType = DamageType.Generic;

			HitPoint = hitPoint;
			HitDirection = hitDirection;
			HitImpulse = hitImpulse;

			HitNormal = Vector3.zero;

			Source = source;
		}

		public HealthEventData(float delta, DamageType damageType, Vector3 hitPoint, Vector3 hitDirection, float hitImpulse, LivingEntity source = null)
		{
			Delta = delta;

			DamageType = damageType;

			HitPoint = hitPoint;
			HitDirection = hitDirection;
			HitImpulse = hitImpulse;

			HitNormal = Vector3.zero;

			Source = source;
		}

		public HealthEventData(float delta, Vector3 hitPoint, Vector3 hitDirection, float hitImpulse, Vector3 hitNormal, LivingEntity source = null)
		{
			Delta = delta;

			DamageType = DamageType.Generic;

			HitPoint = hitPoint;
			HitDirection = hitDirection;
			HitImpulse = hitImpulse;

			HitNormal = hitNormal;

			Source = source;
		}

		public HealthEventData(float delta, DamageType damageType, Vector3 hitPoint, Vector3 hitDirection, float hitImpulse, Vector3 hitNormal, LivingEntity source = null)
		{
			Delta = delta;

			DamageType = damageType;

			HitPoint = hitPoint;
			HitDirection = hitDirection;
			HitImpulse = hitImpulse;

			HitNormal = hitNormal;

			Source = source;
		}
	}
}
