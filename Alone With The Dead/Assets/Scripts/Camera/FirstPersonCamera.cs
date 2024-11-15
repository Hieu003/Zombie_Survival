using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	public class FirstPersonCamera : MonoBehaviour
	{
		#region Internal
		#pragma warning disable 0649

		[Serializable]
		private class SpringsModule
		{
			public float SpringLerpSpeed = 25f;

			[Space]

			[Group]
			public SpringSettings ForceSprings = SpringSettings.Default;

			[Group]
			public SpringSettings HeadbobSprings = SpringSettings.Default;
		}

		[Serializable]
		private struct HeadbobsModule
		{
			[Group]
			public CameraHeadBob WalkHeadbob;

			[Group]
			public CameraHeadBob CrouchHeadbob;

			[Group]
			public CameraHeadBob RunHeadbob;
		}

		[Serializable]
		private struct ShakesModule
		{
			public Spring.Data ShakeSpringSettings;

			public CameraShakeSettings ExplosionShake;

			public CameraShakeSettings DeathShake;
		}

		[Serializable]
		private struct FallImpactModule
		{
			[MinMax(0f, 50f)]
			public Vector2 FallImpactRange;

			public SpringForce PosForce;

			public SpringForce RotForce;
		}

		[Serializable]
		private struct JumpForceModule
		{
			public SpringForce PosForce;

			public SpringForce RotForce;
		}

		[Serializable]
		private struct GettingHitForceModule
		{
			[Range(0f, 10f)]
			public float PosForce;

			[Range(0f, 10f)]
			public float RotForce;
		}

		#pragma warning restore 0649
		#endregion

		public CameraHeadBob AimHeadBob { get; set; }

		public Camera UnityCamera { get { return m_Camera; } }

		[BHeader("General", true)]

		[SerializeField]
		private Camera m_Camera = null;

		[SerializeField]
		private Player m_Player = null;

		[Space]

		[SerializeField, Group()]
		private SpringsModule m_Springs = null;

		[SerializeField, Group()]
		private HeadbobsModule m_Headbobs = new HeadbobsModule();

		[SerializeField, Group()]
		private ShakesModule m_CamShakes = new ShakesModule();

		[SerializeField, Group()]
		private FallImpactModule m_FallImpact = new FallImpactModule();

		[SerializeField, Group()]
		private JumpForceModule m_JumpForce = new JumpForceModule();

		[SerializeField, Group()]
		private GettingHitForceModule m_GettingHitForce = new GettingHitForceModule();

		// Springs
		private Spring m_PositionSpring_Force;
		private Spring m_RotationSpring_Force;
		private Spring m_PositionSpring_Headbob;
		private Spring m_RotationSpring_Headbob;

		private Spring m_PositionShakeSpring;
		private Spring m_RotationShakeSpring;

		private Spring m_PositionRecoilSpring;
		private Spring m_RotationRecoilSpring;

		// Headbob
		private CameraHeadBob m_CurrentHeadbob;
		private float m_CurrentBobParam;

		private Vector3 m_FadeOutBob_Pos;
		private Vector3 m_FadeOutBob_Rot;
		private Vector3 m_FadeInBob_Pos;
		private Vector3 m_FadeInBob_Rot;

		private float m_FadeOutBobMult;
		private float m_FadeInBobMult;
		private float m_FadeInStartTime;
		private int m_LastFootDown;

		// Shakes
		private List<CameraShake> m_Shakes = new List<CameraShake>();


		public bool Raycast(float maxDistance, LayerMask mask, out RaycastHit hitInfo, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
		{
			return Physics.Raycast(new Ray(m_Camera.transform.position, m_Camera.transform.forward), out hitInfo, maxDistance, mask, queryTriggerInteraction);
		}

		public void AdjustRecoilSettings(Spring.Data rotSpringData, Spring.Data posSpringData)
		{
			m_PositionRecoilSpring.Adjust(posSpringData);
			m_RotationRecoilSpring.Adjust(rotSpringData);
		}

		public void ApplyRecoil(RecoilForce force, float forceMultiplier = 1f)
		{
			force.PlayRecoilForce(forceMultiplier, m_RotationRecoilSpring, m_PositionRecoilSpring);
		}

		public void AddPositionForce(Vector3 positionForce, int distribution = 1)
		{
			m_FadeInBobMult = 0f;
			m_FadeInStartTime = Time.time + 1f;

			if (distribution <= 1)
				m_PositionSpring_Force.AddForce(positionForce);
			else
				m_PositionSpring_Force.AddDistributedForce(positionForce, distribution);
		}

		public void AddRotationForce(Vector3 rotationForce, int distribution = 1)
		{
			m_FadeInBobMult = 0f;
			m_FadeInStartTime = Time.time + 1f;

			if (distribution <= 1)
				m_RotationSpring_Force.AddForce(rotationForce);
			else
				m_RotationSpring_Force.AddDistributedForce(rotationForce, distribution);
		}

		public void DoShake(CameraShakeSettings shake, float scale)
		{
			m_Shakes.Add(new CameraShake(shake, m_PositionShakeSpring, m_RotationShakeSpring, scale));
		}

		public void AddExplosionShake(float scale)
		{
			m_Shakes.Add(new CameraShake(m_CamShakes.ExplosionShake, m_PositionShakeSpring, m_RotationShakeSpring, scale));
		}

		public void OnPlayerDeath() 
		{
			m_Shakes.Add(new CameraShake(m_CamShakes.DeathShake, m_PositionShakeSpring, m_RotationShakeSpring, 1f));
		}

		private void Awake()
		{
			// Force Springs
			m_PositionSpring_Force = new Spring(Spring.Type.OverrideLocalPosition, m_Camera.transform);
			m_PositionSpring_Force.Adjust(m_Springs.ForceSprings.Position.Stiffness, m_Springs.ForceSprings.Position.Damping);
			m_RotationSpring_Force = new Spring(Spring.Type.OverrideLocalRotation, m_Camera.transform);
			m_RotationSpring_Force.Adjust(m_Springs.ForceSprings.Rotation.Stiffness, m_Springs.ForceSprings.Rotation.Damping);

			m_PositionSpring_Force.LerpSpeed = m_Springs.SpringLerpSpeed;
			m_RotationSpring_Force.LerpSpeed = m_Springs.SpringLerpSpeed;

			//Headbob Springs
			m_PositionSpring_Headbob = new Spring(Spring.Type.AddToLocalPosition, m_Camera.transform);
			m_PositionSpring_Headbob.Adjust(m_Springs.HeadbobSprings.Position.Stiffness, m_Springs.HeadbobSprings.Position.Damping);
			m_RotationSpring_Headbob = new Spring(Spring.Type.AddToLocalRotation, m_Camera.transform);
			m_RotationSpring_Headbob.Adjust(m_Springs.HeadbobSprings.Rotation.Stiffness, m_Springs.HeadbobSprings.Rotation.Damping);

			m_PositionSpring_Headbob.LerpSpeed = m_Springs.SpringLerpSpeed;
			m_RotationSpring_Headbob.LerpSpeed = m_Springs.SpringLerpSpeed;

			// Shake Springs
			m_PositionShakeSpring = new Spring(Spring.Type.AddToLocalPosition, m_Camera.transform);
			m_PositionShakeSpring.Adjust(m_CamShakes.ShakeSpringSettings);
			m_RotationShakeSpring = new Spring(Spring.Type.AddToLocalRotation, m_Camera.transform);
			m_RotationShakeSpring.Adjust(m_CamShakes.ShakeSpringSettings);

			m_PositionShakeSpring.LerpSpeed = m_Springs.SpringLerpSpeed;
			m_RotationShakeSpring.LerpSpeed = m_Springs.SpringLerpSpeed;

			//Recoil Springs
			m_PositionRecoilSpring = new Spring(Spring.Type.AddToLocalPosition, m_Camera.transform);
			m_PositionRecoilSpring.Adjust(new Vector3(0.02f, 0.02f, 0.02f), new Vector3(0.3f, 0.3f, 0.3f));
			m_RotationRecoilSpring = new Spring(Spring.Type.AddToLocalRotation, m_Camera.transform);
			m_RotationRecoilSpring.Adjust(new Vector3(0.02f, 0.02f, 0.02f), new Vector3(0.3f, 0.3f, 0.3f));

			m_PositionRecoilSpring.LerpSpeed = m_Springs.SpringLerpSpeed;
			m_RotationRecoilSpring.LerpSpeed = m_Springs.SpringLerpSpeed;

			m_Player.FallImpact.AddListener(OnFallImpact);
			m_Player.Jump.AddStartListener(OnStart_Jump);
			m_Player.Aim.AddStartListener(OnAimStart);

			m_Player.MoveCycleEnded.AddListener(OnStepTaken);
			m_Player.MoveCycle.AddChangeListener(OnMovCycleChanged);

			m_Player.ChangeHealth.AddListener(OnPlayerHealthChanged);
			m_Player.Death.AddListener(OnPlayerDeath);

			ShakeManager.ShakeEvent.AddListener(OnShakeEvent);
		}

		private void OnDestroy()
		{
			ShakeManager.ShakeEvent.RemoveListener(OnShakeEvent);
		}

		private void OnAimStart()
		{
			m_CurrentBobParam = 0f;
		}

		private void FixedUpdate()
		{
			m_PositionSpring_Force.FixedUpdate();
			m_RotationSpring_Force.FixedUpdate();

			m_PositionSpring_Headbob.FixedUpdate();
			m_RotationSpring_Headbob.FixedUpdate();

			m_PositionShakeSpring.FixedUpdate();
			m_RotationShakeSpring.FixedUpdate();

			m_PositionRecoilSpring.FixedUpdate();
			m_RotationRecoilSpring.FixedUpdate();

			UpdateHeadbobs(Time.fixedDeltaTime);

			UpdateShakes();
		}

		private void Update()
		{
			m_PositionSpring_Force.Update();
			m_RotationSpring_Force.Update();

			m_PositionSpring_Headbob.Update();
			m_RotationSpring_Headbob.Update();

			m_PositionShakeSpring.Update();
			m_RotationShakeSpring.Update();

			m_PositionRecoilSpring.Update();
			m_RotationRecoilSpring.Update();
		}

		private void UpdateHeadbobs(float deltaTime)
		{
			var previousHeadbob = m_CurrentHeadbob;

			if (m_Player.Run.Active && m_Player.Velocity.Val.sqrMagnitude > 0.2f)
				m_CurrentHeadbob = m_Headbobs.RunHeadbob;
			else if (m_Player.Aim.Active)
				m_CurrentHeadbob = AimHeadBob;
			else if (m_Player.Crouch.Active)
				m_CurrentHeadbob = m_Headbobs.CrouchHeadbob;
			else if (m_Player.Walk.Active)
				m_CurrentHeadbob = m_Headbobs.WalkHeadbob;
			else
			{
				m_CurrentHeadbob = null;
				Easings.Interpolate(ref m_FadeInBobMult, 1f, deltaTime, true);
			}

			if (previousHeadbob != m_CurrentHeadbob && previousHeadbob != null)
			{
				m_FadeOutBob_Pos = m_FadeInBob_Pos * m_FadeInBobMult + m_FadeOutBob_Pos * m_FadeOutBobMult;
				m_FadeOutBob_Rot = m_FadeInBob_Rot * m_FadeInBobMult + m_FadeOutBob_Rot * m_FadeOutBobMult;

				m_FadeInBobMult = 0f;
				m_FadeOutBobMult = 1f;
			}

			if (m_CurrentHeadbob != null && (m_Player.MoveInput.Val != Vector2.zero || m_Player.Aim.Active))
				Easings.Interpolate(ref m_FadeInBobMult, 1f, deltaTime);

			Easings.Interpolate(ref m_FadeOutBobMult, 1f, deltaTime, true);

			m_FadeOutBob_Pos = Vector3.Lerp(m_FadeOutBob_Pos, Vector3.zero, deltaTime * 0.02f);
			m_FadeOutBob_Rot = Vector3.Lerp(m_FadeOutBob_Rot, Vector3.zero, deltaTime * 0.02f);

			if (m_Player.Aim.Active && AimHeadBob != null)
				m_CurrentBobParam += deltaTime * AimHeadBob.HeadBobSpeed;
			else
			{
				m_CurrentBobParam = m_Player.MoveCycle.Val * Mathf.PI;

				if (m_LastFootDown != 0)
					m_CurrentBobParam += Mathf.PI;
			}

			if (Time.time < m_FadeInStartTime)
				m_FadeInBobMult = 0f;

			if (m_CurrentHeadbob != null)
			{
				Vector3 posBobAmplitude = Vector3.zero;
				Vector3 rotBobAmplitude = Vector3.zero;

				// Update position bob
				posBobAmplitude.x = m_CurrentHeadbob.PosAmplitude.x * -0.00001f;
				m_FadeInBob_Pos.x = Mathf.Cos(m_CurrentBobParam) * posBobAmplitude.x;

				posBobAmplitude.y = m_CurrentHeadbob.PosAmplitude.y * 0.00001f;
				m_FadeInBob_Pos.y = Mathf.Cos(m_CurrentBobParam * 2) * posBobAmplitude.y;

				posBobAmplitude.z = m_CurrentHeadbob.PosAmplitude.z * 0.00001f;
				m_FadeInBob_Pos.z = Mathf.Cos(m_CurrentBobParam) * posBobAmplitude.z;

				// Update rotation bob
				rotBobAmplitude.x = m_CurrentHeadbob.RotationAmplitude.x * 0.001f;
				m_FadeInBob_Rot.x = Mathf.Cos(m_CurrentBobParam * 2) * rotBobAmplitude.x;

				rotBobAmplitude.y = m_CurrentHeadbob.RotationAmplitude.y * 0.001f;
				m_FadeInBob_Rot.y = Mathf.Cos(m_CurrentBobParam) * rotBobAmplitude.y;

				rotBobAmplitude.z = m_CurrentHeadbob.RotationAmplitude.z * 0.001f;
				m_FadeInBob_Rot.z = Mathf.Cos(m_CurrentBobParam) * rotBobAmplitude.z;
			}
			else
			{
				m_FadeInBob_Pos = Vector3.Lerp(m_FadeInBob_Pos, Vector3.zero, deltaTime);
				m_FadeInBob_Rot = Vector3.Lerp(m_FadeInBob_Rot, Vector3.zero, deltaTime);
			}

			m_PositionSpring_Headbob.AddForce(m_FadeInBob_Pos * m_FadeInBobMult + m_FadeOutBob_Pos * m_FadeOutBobMult);
			m_RotationSpring_Headbob.AddForce(m_FadeInBob_Rot * m_FadeInBobMult + m_FadeOutBob_Rot * m_FadeOutBobMult);
		}

		private void OnShakeEvent(ShakeEventData shake)
		{
			if (shake.ShakeType == ShakeType.Explosion)
			{
				float distToExplosionSqr = (transform.position - shake.Position).sqrMagnitude;
				float explosionRadiusSqr = shake.Radius * shake.Radius;

				if (explosionRadiusSqr - distToExplosionSqr > 0f)
				{
					float distanceFactor = 1f - Mathf.Clamp01(distToExplosionSqr / explosionRadiusSqr);
					AddExplosionShake(distanceFactor * shake.Scale);
				}
			}
		}

		private void UpdateShakes()
		{
			if (m_Shakes.Count == 0)
				return;

			int i = 0;

			while (true)
			{
				if (m_Shakes[i].IsDone)
					m_Shakes.RemoveAt(i);
				else
				{
					m_Shakes[i].Update();
					i++;
				}

				if (i >= m_Shakes.Count)
					break;
			}
		}

		private void OnPlayerHealthChanged(HealthEventData healthEventData)
		{
			if (healthEventData.Delta < -8f)
			{
				Vector3 posForce = healthEventData.HitDirection == Vector3.zero ? Random.onUnitSphere : healthEventData.HitDirection.normalized;
				posForce *= Mathf.Abs(healthEventData.Delta / 80f);

				Vector3 rotForce = Random.onUnitSphere;

				AddPositionForce(m_Camera.transform.InverseTransformVector(posForce) * m_GettingHitForce.PosForce);
				AddRotationForce(rotForce * m_GettingHitForce.RotForce);
			}
		}

		private void OnFallImpact(float impactVelocity)
		{
			float impactVelocityAbs = Mathf.Abs(impactVelocity);

			if (impactVelocityAbs > m_FallImpact.FallImpactRange.x)
			{
				float multiplier = Mathf.Clamp01(impactVelocityAbs / m_FallImpact.FallImpactRange.y);

				AddPositionForce(m_Camera.transform.InverseTransformVector(m_FallImpact.PosForce.Force) * multiplier, m_FallImpact.PosForce.Distribution);
				AddRotationForce(m_FallImpact.RotForce.Force * multiplier, m_FallImpact.RotForce.Distribution);
			}
		}

		private void OnStart_Jump()
		{
			AddPositionForce(m_JumpForce.PosForce.Force, m_JumpForce.PosForce.Distribution);
			AddRotationForce(m_JumpForce.RotForce.Force, m_JumpForce.RotForce.Distribution);
		}

		private void OnMovCycleChanged(float cycle)
		{
			if (cycle == 0f)
				m_LastFootDown = 0;
			else if (m_Player.MoveCycle.PrevVal == 0f)
			{
				m_FadeOutBob_Pos = m_FadeInBob_Pos * m_FadeInBobMult + m_FadeOutBob_Pos * m_FadeOutBobMult;
				m_FadeOutBob_Rot = m_FadeInBob_Rot * m_FadeInBobMult + m_FadeOutBob_Rot * m_FadeOutBobMult;

				m_FadeInBobMult = 0f;
				m_FadeOutBobMult = 1f;
			}
		}

		private void OnStepTaken()
		{
			m_LastFootDown = m_LastFootDown == 0 ? 1 : 0;
		}

		private void OnValidate()
		{
			if (m_PositionSpring_Force != null && m_RotationSpring_Force != null)
			{
				m_PositionSpring_Force.Adjust(m_Springs.ForceSprings.Position.Stiffness, m_Springs.ForceSprings.Position.Damping);
				m_RotationSpring_Force.Adjust(m_Springs.ForceSprings.Rotation.Stiffness, m_Springs.ForceSprings.Rotation.Damping);
			}
		}
	}

	[Serializable]
	public struct SpringSettings
	{
		public static SpringSettings Default { get { return new SpringSettings() { Position = Spring.Data.Default, Rotation = Spring.Data.Default }; } }

		public Spring.Data Position, Rotation;
	}
}