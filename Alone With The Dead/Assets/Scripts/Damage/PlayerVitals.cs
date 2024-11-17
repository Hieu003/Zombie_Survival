using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// 
	/// </summary>
	public class PlayerVitals : EntityVitals
	{
		public Player Player
		{
			get
			{
				if (!m_Player)
					m_Player = GetComponent<Player>();
				if (!m_Player)
					m_Player = GetComponentInParent<Player>();

				return m_Player;
			}
		}

		[BHeader("Stamina")]

		[SerializeField]
		[Clamp(0f, 300f)]
		private float m_StaminaDepletionRate = 30f;

		[SerializeField]
		private StatRegenData m_StaminaRegeneration = null;

		[SerializeField]
		private SoundPlayer m_BreathingHeavyAudio = null;

		[SerializeField]
		private float m_BreathingHeavyDuration = 11f;

		[SerializeField]
		[Clamp(0f, 100f)]
		private float m_JumpStaminaTake = 15f;

		[BHeader("Player Audio")]

		[SerializeField]
		private SoundPlayer m_JumpAudio = null;

		[SerializeField]
		private SoundPlayer m_CrouchAudio = null;

		[SerializeField]
		private SoundPlayer m_StandUpAudio = null;

		[SerializeField]
		private SoundPlayer m_EarRingingAudio = null;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_EarRingingAudioVolumeDecrease = 0.5f;

		[SerializeField]
		private float m_EarRingVolumeGainSpeed = 0.15f;

		private Player m_Player;
		private float m_LastHeavyBreathTime;


		protected override void Update()
		{
			base.Update();

			// Stamina.
			if (Player.Run.Active)
			{
				m_StaminaRegeneration.Pause();
				ModifyStamina(-m_StaminaDepletionRate * Time.deltaTime);
			}
			else if (m_StaminaRegeneration.CanRegenerate && Player.Stamina.Get() < 100f)
				ModifyStamina(m_StaminaRegeneration.RegenDelta);

			if (!m_StaminaRegeneration.CanRegenerate && Player.Stamina.Is(0f) && Time.time - m_LastHeavyBreathTime > m_BreathingHeavyDuration)
			{
				m_LastHeavyBreathTime = Time.time;
				m_BreathingHeavyAudio.Play2D();
			}

			//Using equipment stops stamina regen for a moment 
			if (Player.UseOnce.LastExecutionTime + 0.1f > Time.time && Player.ActiveEquipmentItem.Val.StaminaTakePerUse > 0)
				m_StaminaRegeneration.Pause();

			AudioListener.volume = Mathf.MoveTowards(AudioListener.volume, 1f, m_EarRingVolumeGainSpeed * Time.deltaTime);
		}

		protected override bool Try_ChangeHealth(HealthEventData healthEventData)
		{
            bool success = base.Try_ChangeHealth(healthEventData);

            // Kiểm tra nếu máu = 0
            if (Player.Health.Get() <= 0f)
            {
                var playerDeath = GetComponent<PlayerDeath>();
                if (playerDeath != null)
                {
                    playerDeath.On_Death(); // Gọi logic chết từ PlayerDeath
                }
            }

            return success;
        }

		protected override void Start()
		{
			Player.Run.AddStartTryer(() => { m_StaminaRegeneration.Pause(); return Player.Stamina.Get() > 0f; });

			Player.Jump.AddStartListener(OnJump);
			Player.Crouch.AddStartListener(OnCrouchStart);
			Player.Crouch.AddStopListener(OnCrouchEnd);

			ShakeManager.ShakeEvent.AddListener(OnShakeEvent);
		}

		private void OnDestroy()
		{
			ShakeManager.ShakeEvent.RemoveListener(OnShakeEvent);
		}

		private void OnShakeEvent(ShakeEventData shake)
		{
			if (shake.ShakeType == ShakeType.Explosion)
			{
				float distToExplosionSqr = (transform.position - shake.Position).sqrMagnitude;
				float explosionRadiusSqr = shake.Radius * shake.Radius;

				float distanceFactor = 1f - Mathf.Clamp01(distToExplosionSqr / explosionRadiusSqr);

				AudioListener.volume = 1f - m_EarRingingAudioVolumeDecrease * distanceFactor;

				m_EarRingingAudio.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource, distanceFactor);
			}
		}

		private void ModifyStamina(float delta)
		{
			float stamina = Player.Stamina.Get() + delta;
			stamina = Mathf.Clamp(stamina, 0f, 100f);
			Player.Stamina.Set(stamina);
		}

		private void OnJump()
		{
			ModifyStamina(-m_JumpStaminaTake);
			m_JumpAudio.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource);

			m_StaminaRegeneration.Pause();
		}

		private void OnCrouchStart()
		{
			m_CrouchAudio.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource);
		}

		private void OnCrouchEnd()
		{
			m_StandUpAudio.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource);
		}

	}     
}
