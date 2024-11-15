using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// Will play a footstep sound when the character travels enough distance on a surface
	/// </summary>
	public class PlayerFootsteps : PlayerComponent
	{
		[SerializeField]
		private LayerMask m_GroundMask = new LayerMask();

		[SerializeField]
		[Range(0f, 1f)]
		private float m_RaycastDistance = 0.2f;

		[Space]

		[SerializeField] 
		[Range(0f, 10f)]
		[Tooltip("If the impact speed is higher than this threeshold, an effect will be played.")]
		private float m_FallImpactThreeshold = 3f;

		[SerializeField] 
		[Range(0f, 1f)]
		private float m_WalkVolume = 1f;

		[SerializeField] 
		[Range(0f, 1f)]
		private float m_CrouchVolume = 1f;

		[SerializeField] 
		[Range(0f, 1f)]
		private float m_RunVolume = 1f;


		private void Start()
		{
			Player.MoveCycleEnded.AddListener(PlayFootstep);
			Player.FallImpact.AddListener(On_FallImpact);
		}
			
		private void PlayFootstep() 
		{
			if (Player.Velocity.Val.sqrMagnitude > 0.1f)
			{
				SurfaceEffects footstepEffect = SurfaceEffects.SoftFootstep;

				if (Player.Run.Active)
					footstepEffect = SurfaceEffects.HardFootstep;

				float volumeFactor = m_WalkVolume;

				if (Player.Crouch.Active)
					volumeFactor = m_CrouchVolume;
				else if (Player.Run.Active)
					volumeFactor = m_RunVolume;

				RaycastHit hitInfo;

				if (CheckGround(out hitInfo))
					SurfaceManager.SpawnEffect(hitInfo, footstepEffect, volumeFactor);
			}
		}

		private void On_FallImpact(float fallImpactSpeed)
		{
			// Don't play the clip when the impact speed is low
			bool wasHardImpact = Mathf.Abs(fallImpactSpeed) >= m_FallImpactThreeshold;
	
			if(wasHardImpact)
			{
				RaycastHit hitInfo;
				if(CheckGround(out hitInfo))
					SurfaceManager.SpawnEffect(hitInfo, SurfaceEffects.FallImpact, 1f);
			}
		}

		private bool CheckGround(out RaycastHit hitInfo)
		{
			Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);

			return Physics.Raycast(ray, out hitInfo, m_RaycastDistance, m_GroundMask, QueryTriggerInteraction.Ignore);
		}
	}
}