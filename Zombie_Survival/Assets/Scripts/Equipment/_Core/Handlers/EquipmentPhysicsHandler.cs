using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	public class EquipmentPhysicsHandler : PlayerComponent
	{
		#region Internal
		[Serializable]
		private class GeneralSettings
		{
			public float SpringLerpSpeed = 25f;
			public float PositionBobOffset = 0f;
			public float RotationBobOffset = 0.5f;
			public float SpringForceMultiplier = 1f;
		}
		#endregion

		public Spring PositionSpring { get; private set; }
		public Spring RotationSpring { get; private set; }

		[SerializeField]
		[Group]
		private GeneralSettings m_GeneralSettings = null;

		private EquipmentHandler m_FPHandler;

		private Transform m_Pivot;
		private Transform m_Model;
		private Transform m_Root;

		private Vector3 m_OriginalRootPosition;
		private Quaternion m_OriginalRootRotation;

		private Vector3 m_ModelOriginalPos;

		private Vector2 m_LookInput;

		// Sway
		private Vector3 m_SwayVelocity;

		// Motion states
		private EquipmentMotionState m_CurrentState;
		private EquipmentPhysics m_EquipmentPhysics;

		private float m_ChangeToDefaultOffestTime;
		private float m_LerpedOffset;

		private Vector3 m_StatePosition;
		private Vector3 m_StateRotation;

		// Bob
		private int m_LastFootDown;
		private float m_CurrentBobParam;

		// State visualization
		private EquipmentMotionState m_StateToVisualize = null;
		private float m_VisualizationSpeed = 4f;
		private bool m_FirstStepTriggered;


		public void SetStateToVisualize(EquipmentMotionState state, float speed)
		{
			m_StateToVisualize = state;
			m_VisualizationSpeed = speed;
			m_CurrentBobParam = 0f;
		}

		public void OnValidate()
		{
			if (Application.isPlaying)
			{
				if (m_CurrentState != null)
				{
					PositionSpring.Adjust(m_CurrentState.PositionSpring);
					RotationSpring.Adjust(m_CurrentState.RotationSpring);
				}

				if (m_Root != null)
					SetOffset();
			}
		}

		public void ResetSpring()
		{
			PositionSpring.Reset();
			RotationSpring.Reset();

			m_StatePosition = m_StateRotation = Vector3.zero;

			m_Pivot.localPosition = Vector3.zero;
			m_Pivot.localRotation = Quaternion.identity;

			m_CurrentState = null;
		}

		public void SetOffset()
		{
			m_Root.localPosition = m_OriginalRootPosition + m_EquipmentPhysics.GeneralSettings.BasePosOffset;
			m_Root.localRotation = Quaternion.Euler(m_OriginalRootRotation.eulerAngles + m_EquipmentPhysics.GeneralSettings.BaseRotOffset);
		}

		private void Awake()
		{
			Player.FallImpact.AddListener(On_FallImpact);
			Player.MoveCycleEnded.AddListener(On_StepTaken);
			Player.Death.AddListener(ResetSpring);
			Player.Jump.AddStartListener(On_Jump);

			//Find the pivot
			m_Pivot = transform.FindDeepChild("Pivot");
			if (m_Pivot == null)
			{
				Debug.LogError("You have no pivot under this object. A pivot is an empty game object with the name 'Pivot'");
				enabled = false;
				return;
			}

			//Find the model
			m_Model = transform.FindDeepChild("Model");
			if (m_Model == null)
			{
				Debug.LogError("You have no model under this object. The equipment model should have the name 'Model'");
				enabled = false;
				return;
			}

			m_ModelOriginalPos = m_Model.localPosition;

			m_Pivot.localRotation = Quaternion.identity;

			var root = new GameObject("Root");
			m_Root = root.transform;
			m_Root.SetParent(transform, true);
			m_Root.position = m_Pivot.position;
			m_Root.rotation = m_Pivot.rotation;

			m_Pivot.SetParent(m_Root.transform, true);
			m_Model.SetParent(m_Pivot, true);

			PositionSpring = new Spring(Spring.Type.OverrideLocalPosition, m_Pivot, m_Pivot.transform.localPosition);
			RotationSpring = new Spring(Spring.Type.OverrideLocalRotation, m_Pivot, m_Pivot.transform.localEulerAngles);
		}

		private void Start()
		{
			m_FPHandler = GetComponent<EquipmentHandler>();
			m_FPHandler.OnSelected.AddListener(On_ChangeItem);
			m_FPHandler.UsingItem.AddStopListener(OnUseEnd);

			On_ChangeItem(true);

			RotationSpring.LerpSpeed = m_GeneralSettings.SpringLerpSpeed;
			PositionSpring.LerpSpeed = m_GeneralSettings.SpringLerpSpeed;
		}

		private void OnUseEnd()
		{
			if (m_CurrentState == null && PositionSpring != null)
				return;

			PositionSpring.Adjust(m_CurrentState.PositionSpring);
			RotationSpring.Adjust(m_CurrentState.RotationSpring);
		}

		private void FixedUpdate()
		{
			if (m_EquipmentPhysics == null)
				return;

			m_LookInput = Player.LookInput.Get();

			m_LookInput *= m_EquipmentPhysics.Input.LookInputMultiplier;
			m_LookInput = Vector2.ClampMagnitude(m_LookInput, m_EquipmentPhysics.Input.MaxLookInput);

			m_StatePosition = Vector3.zero;
			m_StateRotation = Vector3.zero;

			UpdateState();

			UpdateOffset();
			UpdateBob(Time.fixedDeltaTime);
			UpdateSway();

			UpdateNoise();

			m_StatePosition *= m_GeneralSettings.SpringForceMultiplier;
			m_StateRotation *= m_GeneralSettings.SpringForceMultiplier;

			PositionSpring.AddForce(m_StatePosition);
			RotationSpring.AddForce(m_StateRotation);

			if (PositionSpring != null && RotationSpring != null)
			{
				RotationSpring.FixedUpdate();
				PositionSpring.FixedUpdate();
			}
		}

		private void Update()
		{
			if (PositionSpring != null && RotationSpring != null)
			{
				RotationSpring.Update();
				PositionSpring.Update();
			}
		}

		private void UpdateState()
		{
			if (m_StateToVisualize != null)
				TrySetState(m_StateToVisualize);
			else
			{
				if (Player.Run.Active && Player.Velocity.Val.sqrMagnitude > 0.2f)
					TrySetState(m_EquipmentPhysics.RunState);
				else if (Player.Aim.Active)
					TrySetState(m_EquipmentPhysics.AimState);
				else if (Player.Crouch.Active)
					TrySetState(m_EquipmentPhysics.CrouchState);
				else if (Player.Walk.Active && Player.Velocity.Val.sqrMagnitude > 0.2f)
					TrySetState(m_EquipmentPhysics.WalkState);
				else
					TrySetState(m_EquipmentPhysics.IdleState);
			}
		}

		private void TrySetState(EquipmentMotionState state)
		{
			if (m_CurrentState != state)
			{
				if (m_CurrentState != null)
				{
					if ((m_CurrentState.HasEntryOffset && m_ChangeToDefaultOffestTime < Time.time) || !m_CurrentState.HasEntryOffset)
					{
						if (!(m_CurrentState == m_EquipmentPhysics.CrouchState && state == m_EquipmentPhysics.AimState))
						{
							RotationSpring.AddForce(m_CurrentState.ExitForce);
							PositionSpring.AddForce(m_CurrentState.PosExitForce);
						}
					}
				}

				m_CurrentState = state;

				PositionSpring.Adjust(state.PositionSpring);
				RotationSpring.Adjust(state.RotationSpring);

				if (m_CurrentState != null)
				{
					if (m_CurrentState.HasEntryOffset)
						m_ChangeToDefaultOffestTime = Time.time + m_CurrentState.EntryOffsetDuration;

					m_LerpedOffset = 0f;

					RotationSpring.AddForce(m_CurrentState.EnterForce);
					PositionSpring.AddForce(m_CurrentState.PosEnterForce);
				}
			}
		}

		private void UpdateBob(float deltaTime)
		{
			if (!m_CurrentState.Bob.Enabled || (Player.Velocity.Get().sqrMagnitude == 0 && m_StateToVisualize == null && m_CurrentState == m_EquipmentPhysics.AimState))
				return;

			if (m_StateToVisualize != null)
			{
				m_CurrentBobParam += deltaTime * m_VisualizationSpeed * 2;

				if (!m_FirstStepTriggered && m_CurrentBobParam >= Mathf.PI)
				{
					m_FirstStepTriggered = true;
					ApplyStepForce();
				}

				if (m_CurrentBobParam >= Mathf.PI * 2f)
				{
					m_CurrentBobParam -= Mathf.PI * 2f;
					m_FirstStepTriggered = false;
					ApplyStepForce();
				}
			}
			else
			{
				m_CurrentBobParam = Player.MoveCycle.Get() * Mathf.PI;

				if (m_LastFootDown != 0)
					m_CurrentBobParam += Mathf.PI;
			}

			// Update position bob
			Vector3 posBobAmplitude = Vector3.zero;

			posBobAmplitude.x = m_CurrentState.Bob.PositionAmplitude.x * -0.00001f;
			posBobAmplitude.y = m_CurrentState.Bob.PositionAmplitude.y * 0.00001f;
			posBobAmplitude.z = m_CurrentState.Bob.PositionAmplitude.z * 0.00001f;

			m_StatePosition.x += Mathf.Cos(m_CurrentBobParam + m_GeneralSettings.PositionBobOffset) * posBobAmplitude.x;
			m_StatePosition.y += Mathf.Cos(m_CurrentBobParam * 2 + m_GeneralSettings.PositionBobOffset) * posBobAmplitude.y;
			m_StatePosition.z += Mathf.Cos(m_CurrentBobParam + m_GeneralSettings.PositionBobOffset) * posBobAmplitude.z;

			// Update rotation bob
			Vector3 rotBobAmplitude = m_CurrentState.Bob.RotationAmplitude * 0.001f;

			m_StateRotation.x += Mathf.Cos(m_CurrentBobParam * 2 + m_GeneralSettings.RotationBobOffset) * rotBobAmplitude.x;
			m_StateRotation.y += Mathf.Cos(m_CurrentBobParam + m_GeneralSettings.RotationBobOffset) * rotBobAmplitude.y;
			m_StateRotation.z += Mathf.Cos(m_CurrentBobParam + m_GeneralSettings.RotationBobOffset) * rotBobAmplitude.z;
		}

		private void UpdateOffset()
		{
			if (!m_CurrentState.Offset.Enabled || Player.Reload.Active)
				return;

			if (m_CurrentState.HasEntryOffset)
			{
				if (m_ChangeToDefaultOffestTime > Time.time)
				{
					m_StatePosition += m_CurrentState.EntryOffset.PositionOffset * 0.0001f;
					m_StateRotation += m_CurrentState.EntryOffset.RotationOffset * 0.02f;
				}
				else
				{
					m_LerpedOffset = Mathf.Lerp(m_LerpedOffset, 1, Time.deltaTime * m_CurrentState.LerpToDefaultOffestSpeed);

					m_StatePosition += m_CurrentState.Offset.PositionOffset * 0.0001f * m_LerpedOffset;
					m_StateRotation += m_CurrentState.Offset.RotationOffset * 0.02f * m_LerpedOffset;
				}
			}
			else
			{
				m_StatePosition += m_CurrentState.Offset.PositionOffset * 0.0001f;
				m_StateRotation += m_CurrentState.Offset.RotationOffset * 0.02f;
			}
		}

		private void UpdateSway()
		{
			if (!m_EquipmentPhysics.Sway.Enabled)
				return;

			float multiplier = Player.Aim.Active ? m_EquipmentPhysics.Sway.AimMultiplier : 1f;
			multiplier *= Time.fixedDeltaTime;

			m_SwayVelocity = Player.Velocity.Get();

			if (Mathf.Abs(m_SwayVelocity.y) < 1.5f)
				m_SwayVelocity.y = 0f;

			Vector3 localVelocity = transform.InverseTransformDirection(m_SwayVelocity / 60);

			// Look position sway
			PositionSpring.AddForce(new Vector3(
				m_LookInput.x * m_EquipmentPhysics.Sway.LookPositionSway.x * 0.125f,
				m_LookInput.y * m_EquipmentPhysics.Sway.LookPositionSway.y * -0.125f,
				m_LookInput.y * m_EquipmentPhysics.Sway.LookPositionSway.z * -0.125f) * multiplier);

			// Look rotation sway
			RotationSpring.AddForce(new Vector3(
				m_LookInput.y * m_EquipmentPhysics.Sway.LookRotationSway.x * 1.25f,
				m_LookInput.x * m_EquipmentPhysics.Sway.LookRotationSway.y * -1.25f,
				m_LookInput.x * m_EquipmentPhysics.Sway.LookRotationSway.z * -1.25f) * multiplier);

			// Falling
			var fallSway = m_EquipmentPhysics.Sway.FallSway * m_SwayVelocity.y * 0.2f * multiplier;
			if (Player.IsGrounded.Get())
				fallSway *= (15f * multiplier);

			fallSway.z = Mathf.Max(0f, m_EquipmentPhysics.Sway.FallSway.z);
			RotationSpring.AddForce(fallSway);

			// Strafe position sway
			PositionSpring.AddForce(new Vector3(
				localVelocity.x * m_EquipmentPhysics.Sway.StrafePositionSway.x * 0.08f,
				-Mathf.Abs(localVelocity.x * m_EquipmentPhysics.Sway.StrafePositionSway.y * 0.08f),
				-localVelocity.z * m_EquipmentPhysics.Sway.StrafePositionSway.z * 0.08f) * multiplier);

			// Strafe rotation sway
			RotationSpring.AddForce(new Vector3(
				-Mathf.Abs(localVelocity.x * m_EquipmentPhysics.Sway.StrafeRotationSway.x * 8f),
				-localVelocity.x * m_EquipmentPhysics.Sway.StrafeRotationSway.y * 8f,
				localVelocity.x * m_EquipmentPhysics.Sway.StrafeRotationSway.z * 8f) * multiplier);
		}

		private void UpdateNoise()
		{
			if (m_CurrentState.Noise.PosNoiseAmplitude != Vector3.zero && m_CurrentState.Noise.RotNoiseAmplitude != Vector3.zero)
			{
				float jitter = Random.Range(0, m_CurrentState.Noise.MaxJitter);
				float timeScale = Time.time * m_CurrentState.Noise.NoiseSpeed;

				m_StatePosition.x += (Mathf.PerlinNoise(jitter, timeScale) - 0.5f) * m_CurrentState.Noise.PosNoiseAmplitude.x / 1000;
				m_StatePosition.y += (Mathf.PerlinNoise(jitter + 1f, timeScale) - 0.5f) * m_CurrentState.Noise.PosNoiseAmplitude.y / 1000;
				m_StatePosition.z += (Mathf.PerlinNoise(jitter + 2f, timeScale) - 0.5f) * m_CurrentState.Noise.PosNoiseAmplitude.z / 1000;

				m_StateRotation.x += (Mathf.PerlinNoise(jitter, timeScale) - 0.5f) * m_CurrentState.Noise.RotNoiseAmplitude.x / 10;
				m_StateRotation.y += (Mathf.PerlinNoise(jitter + 1f, timeScale) - 0.5f) * m_CurrentState.Noise.RotNoiseAmplitude.y / 10;
				m_StateRotation.z += (Mathf.PerlinNoise(jitter + 2f, timeScale) - 0.5f) * m_CurrentState.Noise.RotNoiseAmplitude.z / 10;
			}
		}

		private void On_ChangeItem(bool selected)
		{
            if (selected)
            {
                m_EquipmentPhysics = m_FPHandler.ItemTransform.GetComponent<EquipmentPhysics>();

				ResetSpring();

				m_Model.SetParent(transform);
				m_Pivot.SetParent(transform);

				m_Pivot.localPosition = m_EquipmentPhysics.GeneralSettings.PivotPosition;
				m_Pivot.localRotation = Quaternion.identity;

				m_Root.position = m_Pivot.position;
				m_Root.rotation = m_Pivot.rotation;

				m_Model.localPosition = m_ModelOriginalPos;
				m_Model.localRotation = Quaternion.identity;

				m_OriginalRootPosition = m_Root.localPosition; 
				m_OriginalRootRotation = m_Root.localRotation;

				m_Pivot.SetParent(m_Root, true);
				m_Model.SetParent(m_Pivot, true);

				PositionSpring = new Spring(Spring.Type.OverrideLocalPosition, m_Pivot, Vector3.zero);
				RotationSpring = new Spring(Spring.Type.OverrideLocalRotation, m_Pivot, Vector3.zero);

				SetOffset();
			}
		}

		private void On_Jump() 
		{
			if (m_EquipmentPhysics == null) return;

			RotationSpring.AddDistributedForce(m_EquipmentPhysics.Sway.JumpForce.Force * 0.01f, m_EquipmentPhysics.Sway.JumpForce.Distribution);
		}

		private void On_FallImpact(float impactSpeed)
		{
			if (m_EquipmentPhysics == null) return;

			impactSpeed *= Player.Aim.Active ? 0.5f : 1f;

			PositionSpring.AddDistributedForce(m_Pivot.InverseTransformVector(m_EquipmentPhysics.FallImpact.PositionForce.Force) * impactSpeed * 0.0001f, m_EquipmentPhysics.FallImpact.PositionForce.Distribution);
			RotationSpring.AddDistributedForce(m_EquipmentPhysics.FallImpact.RotationForce.Force * impactSpeed, m_EquipmentPhysics.FallImpact.RotationForce.Distribution);
		}

		private void On_StepTaken()
		{
			if (Player.Velocity.Val.sqrMagnitude > 0.2f && m_EquipmentPhysics != null)
				ApplyStepForce();

			m_LastFootDown = m_LastFootDown == 0 ? 1 : 0;
		}

		private void ApplyStepForce()
		{
			EquipmentPhysics.StepForceModule stepForce = null;

			if (Player.Walk.Active || m_StateToVisualize == m_EquipmentPhysics.WalkState)
				stepForce = m_EquipmentPhysics.WalkStepForce;

			if (Player.Crouch.Active || m_StateToVisualize == m_EquipmentPhysics.CrouchState)
				stepForce = m_EquipmentPhysics.CrouchStepForce;

			if (Player.Run.Active || m_StateToVisualize == m_EquipmentPhysics.RunState)
				stepForce = m_EquipmentPhysics.RunStepForce;

			if (stepForce != null && stepForce.Enabled && !Player.Aim.Active)
			{
				PositionSpring.AddForce(stepForce.PositionForce.Force * 0.0001f, stepForce.PositionForce.Distribution);
				RotationSpring.AddForce(stepForce.RotationForce.Force * 0.01f, stepForce.RotationForce.Distribution);
			}
		}
    }
}