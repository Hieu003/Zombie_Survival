using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HQFPSWeapons
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	public class ShaftedProjectile : Projectile
	{
		public struct ImpactInfo
		{
			public Hitbox Hitbox;
		}

		public Message<ImpactInfo> Impact = new Message<ImpactInfo>();
		public Message Launched = new Message();

		[BHeader("General", true)]

		[SerializeField]
		private LayerMask m_Mask = new LayerMask();

		[SerializeField]
		private float m_MaxDistance = 2f;

		[SerializeField]
		private TrailRenderer m_Trail = null;

		[SerializeField]
		private bool m_AllowSticking = false;

		[BHeader("Damage")]

		[SerializeField]
		[Range(0f, 200f)]
		private float m_MaxDamageSpeed = 50f;

		[SerializeField]
		[Range(0f, 200f)]
		private float m_MaxDamage = 100f;

		[SerializeField]
		private AnimationCurve m_DamageCurve = new AnimationCurve(
			new Keyframe(0f, 1f),
			new Keyframe(0.8f, 0.5f),
			new Keyframe(1f, 0f));

		[SerializeField]
		private float m_ImpactForce = 15f;

		[BHeader("Penetration")]

		[SerializeField]
		private float m_PenetrationOffset = 0.2f;

		[SerializeField]
		private Vector2 m_RandomRotation = Vector2.zero;

		[SerializeField]
		private bool m_PlayTwangAnimation = false;

		[SerializeField]
		private UnityEvent m_OnImpact = null;

		[SerializeField]
		[ShowIf("m_PlayTwangAnimation", true)]
		private TwangSettings m_TwangSettings = null;

		[BHeader("Audio")]

		[SerializeField]
		private AudioSource m_AudioSource = null;

		[SerializeField]
		private SurfaceEffects m_PenetrationEffect = new SurfaceEffects();

		[BHeader("Pickup")]

		[SerializeField]
		private bool m_EnablePickupOnImpact = true;

		[SerializeField]
		private ItemPickup m_Pickup = null;

		private LivingEntity m_Launcher;
		private Collider m_Collider;
		private Rigidbody m_Rigidbody;
		private bool m_Done;
		private bool m_Launched;

		private Transform m_Pivot;


		public override void Launch(LivingEntity launcher)
		{
			if (m_Launcher != null)
			{
				Debug.LogWarningFormat(this, "Already launched this projectile!", name);
				return;
			}

			m_Launcher = launcher;
			m_Launched = true;

			OnLaunched();

			Launched.Send();
		}

		public void CheckForSurfaces(Vector3 position, Vector3 direction)
		{
			RaycastHit hitInfo;
			Ray ray = new Ray(position, direction);

			if (Physics.Raycast(ray, out hitInfo, m_MaxDistance, m_Mask, QueryTriggerInteraction.Ignore))
			{
				SurfaceManager.SpawnEffect(hitInfo, SurfaceEffects.Stab, 1f);

				float currentSpeed = m_Rigidbody.velocity.magnitude;

				float impulse = m_ImpactForce;
				var damageable = hitInfo.collider.GetComponent<IDamageable>();

				// If the object is damageable...
				if (damageable != null)
				{
					float damageMod = m_DamageCurve.Evaluate(1f - currentSpeed / m_MaxDamageSpeed);
					float damage = m_MaxDamage * damageMod;

					var damageData = new HealthEventData(-damage, DamageType.Stab, hitInfo.point, ray.direction, impulse, hitInfo.normal, m_Launcher);
					damageable.TakeDamage(damageData);
				}

				if (hitInfo.rigidbody != null)
				{
					// If the object is a rigidbody, apply an impact force
					hitInfo.rigidbody.AddForceAtPosition(transform.forward * impulse, hitInfo.point, ForceMode.Impulse);
				}

				SurfaceManager.SpawnEffect(hitInfo, m_PenetrationEffect, 1f);

				// Stick the projectile in the object
				transform.position = hitInfo.point + transform.forward * m_PenetrationOffset;

				var hitbox = hitInfo.collider.GetComponent<Hitbox>();

				m_OnImpact.Invoke();

				Impact.Send(new ImpactInfo() { Hitbox = hitbox });

				m_Collider.enabled = true;

				if (m_AllowSticking && m_Pickup != null)
				{
					transform.SetParent(hitInfo.transform);

					OnSurfacePenetrated();

					Physics.IgnoreCollision(m_Collider, hitInfo.collider);
				}

				m_Done = true;
			}
		}

		public void OnLaunched()
		{
			if (m_Pickup != null)
				m_Pickup.EnablePickup(false);

			m_Collider.enabled = false;

			if(m_Trail != null)
				m_Trail.enabled = true;
		}

		public void OnSurfacePenetrated()
		{
			m_Rigidbody.isKinematic = true;

			if (m_PlayTwangAnimation)
				StartCoroutine(C_DoTwang());

			if (m_Pickup != null && m_EnablePickupOnImpact)
				m_Pickup.EnablePickup(true);
		}

		private void Awake()
		{
			m_Collider = GetComponent<Collider>();
			m_Rigidbody = GetComponent<Rigidbody>();

			if(m_Trail != null)
				m_Trail.enabled = false;
		}

		private void FixedUpdate()
		{
			if (m_Launched && !m_Done)
				CheckForSurfaces(transform.position, transform.forward);
		}

		#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			if (m_PlayTwangAnimation)
			{
				Vector3 twangPivotPosition = transform.position + transform.TransformVector(m_TwangSettings.MovementPivot);

				Gizmos.color = new Color(1f, 0f, 0f, 0.85f);
				Gizmos.DrawSphere(twangPivotPosition, 0.03f);

				Vector3 sceneCamPosition = SceneView.currentDrawingSceneView.camera.transform.position;
				Vector3 sceneCamForward = SceneView.currentDrawingSceneView.camera.transform.forward;

				// Make sure we don't draw the label when not looking at it
				if (Vector3.Dot(sceneCamForward, twangPivotPosition - sceneCamPosition) >= 0f)
					Handles.Label(twangPivotPosition, "Twang Pivot");
			}
		}
		#endif

		private IEnumerator C_DoTwang()
		{
			m_Pivot = new GameObject("Shafted Projectile Pivot").transform;
			m_Pivot.position = transform.position + Vector3Utils.LocalToWorld(m_TwangSettings.MovementPivot, transform);
			m_Pivot.rotation = transform.rotation;

			float stopTime = Time.time + m_TwangSettings.Duration;
			float range = m_TwangSettings.Range;
			float currentVelocity = 0f;

			m_TwangSettings.Audio.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource);

			Quaternion localRotation = m_Pivot.localRotation;

			Quaternion randomRotation = Quaternion.Euler(new Vector2(
				Random.Range(-m_RandomRotation.x, m_RandomRotation.x),
				Random.Range(-m_RandomRotation.y, m_RandomRotation.y)));

			while (Time.time < stopTime)
			{
				m_Pivot.localRotation = localRotation * randomRotation * Quaternion.Euler(Random.Range(-range, range), Random.Range(-range, range), 0f);
				range = Mathf.SmoothDamp(range, 0f, ref currentVelocity, stopTime - Time.time);

				yield return null;
			}
		}

		private void OnDestroy()
		{
			if (m_Pivot != null)
				Destroy(m_Pivot.gameObject);
		}

        #region Internal
        [Serializable]
		public class TwangSettings
		{
			public Vector3 MovementPivot;

			public float Duration = 1f;

			public float Range = 18f;

			public SoundPlayer Audio;
		}
        #endregion
    }
}