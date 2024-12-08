using System;
using UnityEngine;

namespace HQFPSWeapons
{
	public class EquipmentPhysics : FPEquipmentComponent
	{
		#region Internal
		public enum StateType
		{
			None = 0,
			Idle = 5,
			Walk = 10,
			Crouch = 12,
			Run = 15,
			Aim = 20,
			OnLadder = 25,
			Retraction = 30
		}

		[Serializable]
		public class BaseSettings
		{
			public Vector3 PivotPosition = new Vector3(-0.006f, -0.1715f, 0.4f);

			public Vector3 BasePosOffset;
			public Vector3 BaseRotOffset;
		}
		[Serializable]
		public class InputModule
		{
			[Range(0f, 20f)]
			public float LookInputMultiplier = 1f;

			[Clamp(0f, Mathf.Infinity)]
			public float MaxLookInput = 20f;
		}

		[Serializable]
		public class SwayModule : CloneableObject<SwayModule>
		{
			public bool Enabled = true;

			[Space]

			public float AimMultiplier = 0.2f;

			[Space]

			public Vector3 LookPositionSway;

			public Vector3 LookRotationSway;

			public Vector3 FallSway;

			[Space]

			public Vector3 StrafePositionSway;

			public Vector3 StrafeRotationSway;

			[Space]

			public SpringForce JumpForce;
		}

		[Serializable]
		public class FallImpactModule : CloneableObject<FallImpactModule>
		{
			public bool Enabled = true;

			[Space]

			public SpringForce PositionForce;

			public SpringForce RotationForce;
		}

		[Serializable]
		public class StepForceModule : CloneableObject<StepForceModule>
		{
			public bool Enabled = true;

			[Space]

			public SpringForce PositionForce;

			public SpringForce RotationForce;
		}
		#endregion

		[BHeader("GENERAL", true)]

		[Group]
		public BaseSettings GeneralSettings = null;

		[Space]

		[Group]
		public InputModule Input = null;

		[Group]
		public SwayModule Sway = null;

		[Group]
		public FallImpactModule FallImpact = null;

		[Space(3f)]

		[BHeader("STEP FORCES", true, order = 100)]

		[Group]
		public StepForceModule WalkStepForce = null;

		[Group]
		public StepForceModule CrouchStepForce = null;

		[Group]
		public StepForceModule RunStepForce = null;

		[Space(3f)]

		[BHeader("STATES", true, order = 100)]

		public EquipmentMotionState IdleState = null;

		public EquipmentMotionState WalkState = null;

		public EquipmentMotionState RunState = null;

		public EquipmentMotionState AimState = null;

		public EquipmentMotionState CrouchState = null;


		#if UNITY_EDITOR
        public void VisualizeState(StateType stateType, float speed = 4f)
		{
			if (m_EHandler == null)
				return;

			if (stateType == StateType.Idle)
				m_EHandler.PhysicsHandler.SetStateToVisualize(IdleState, speed);
			else if (stateType == StateType.Walk)
				m_EHandler.PhysicsHandler.SetStateToVisualize(WalkState, speed);
			else if (stateType == StateType.Run)
				m_EHandler.PhysicsHandler.SetStateToVisualize(RunState, speed);
			else if (stateType == StateType.Aim)
				m_EHandler.PhysicsHandler.SetStateToVisualize(AimState, speed);
			else if (stateType == StateType.Crouch)
				m_EHandler.PhysicsHandler.SetStateToVisualize(CrouchState, speed);
			else if (stateType == StateType.None)
				m_EHandler.PhysicsHandler.SetStateToVisualize(null, speed);
		}
		#endif

		private void OnValidate()
		{
			if (m_EHandler == null)
				return;

			m_EHandler.PhysicsHandler.OnValidate();
		}
	}
}