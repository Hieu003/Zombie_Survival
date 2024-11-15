using UnityEngine;
using System;

namespace HQFPSWeapons
{
	[Serializable]
	public class DamageResistance
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float m_GenericResistance = 0.1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_CutResistance = 0.1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_HitResistance = 0.1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_StabResistance = 0.1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_BulletResistance = 0.1f;


		public float GetDamageResistance(HealthEventData damageData)
		{
			if(damageData.DamageType == DamageType.Generic)
				return m_GenericResistance;
			else if(damageData.DamageType == DamageType.Cut)
				return m_CutResistance;
			else if(damageData.DamageType == DamageType.Hit)
				return m_HitResistance;
			else if(damageData.DamageType == DamageType.Stab)
				return m_StabResistance;
			else if(damageData.DamageType == DamageType.Bullet)
				return m_BulletResistance;

			return 0f;
		}
	}

	[Serializable]
	public class StatRegenData
	{
		public bool CanRegenerate { get { return m_Enabled && !IsPaused; } }

		public bool Enabled { get { return m_Enabled; } }

		public bool IsPaused { get { return Time.time < m_NextRegenTime; } }

		public float RegenDelta { get { return m_Speed * Time.deltaTime; } }

		[SerializeField]
		private bool m_Enabled = true;

		[SerializeField]
		private float m_Pause = 2f;

		[SerializeField]
		[Clamp(0f, 1000f)]
		private float m_Speed = 10f;

		private float m_NextRegenTime;


		public void Pause()
		{
			m_NextRegenTime = Time.time + m_Pause;
		}
	}

    public class GenericVitals : LivingEntityComponent
    {
		[SerializeField]
		private DamageResistance m_DamageResistance = null;

        [BHeader("Health & Damage")]

        [SerializeField]
        [Tooltip("The health to start with.")]
        private float m_MaxHealth = 100f;

		[SerializeField]
		private StatRegenData m_HealthRegeneration = null;

		[BHeader("Audio")]

		[SerializeField]
		protected AudioSource m_AudioSource = null;

		protected float m_HealthDelta;
        

		protected virtual void Start() 
		{
			Entity.ChangeHealth.SetTryer(Try_ChangeHealth);

			SetOriginalMaxHealth();
		}

        protected virtual void Update()
		{
			if(m_HealthRegeneration.CanRegenerate && Entity.Health.Get() < 100f && Entity.Health.Get() > 0f)
			{
				var data = new HealthEventData(m_HealthRegeneration.RegenDelta);
				Entity.ChangeHealth.Try(data);
			}
		}

		protected virtual bool Try_ChangeHealth(HealthEventData healthEventData)
		{
			if(Entity.Health.Get() == 0f)
				return false;
			if(healthEventData.Delta > 0f && Entity.Health.Get() == 100f)
				return false;

			float healthDelta = healthEventData.Delta;

			if(healthDelta < 0f)
				healthDelta *= (1f - m_DamageResistance.GetDamageResistance(healthEventData));

			float newHealth = Mathf.Clamp(Entity.Health.Get() + healthDelta, 0f, 100f);
			Entity.Health.Set(newHealth);

			if(healthDelta < 0f)
				m_HealthRegeneration.Pause();

			return true;
		}

		private void SetOriginalMaxHealth() 
		{
			Entity.Health.Set(m_MaxHealth);
		}
    }
}