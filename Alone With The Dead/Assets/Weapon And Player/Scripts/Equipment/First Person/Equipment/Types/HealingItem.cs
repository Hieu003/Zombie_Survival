using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
    public class HealingItem : EquipmentItem
    {
        [BHeader("HEALING ITEM", true)]

        [SerializeField]
        [Group]
        private HealingSettings m_HealingSettings = null;

        [Space]

        [SerializeField]
        [Group]
        public DelayedCameraForce[] m_HealingCameraForces;

        [SerializeField]
        [Group]
        public DelayedSound[] m_HealingAudio;

        private bool m_IsHealing;


        public override void Wield(SaveableItem item)
        {
            if (m_IsHealing)
                return;

            m_IsHealing = true;

            m_EHandler.Animator.SetFloat("Use Speed", m_HealingSettings.UseAnimSpeed);
            m_EHandler.Animator.SetTrigger("Use");

            StartCoroutine(C_StartHealing());
        }

        public override void Unwield()
        {
            
        }

        IEnumerator C_StartHealing() 
        {
            float healDelay = m_HealingSettings.GiveHealthDelay * (1 / m_HealingSettings.UseAnimSpeed);
            float delay = m_HealingSettings.GiveHealthDelay - healDelay;

            m_EHandler.PlayCameraForces(m_HealingCameraForces);
            m_EHandler.PlaySounds(m_HealingAudio);

            m_EHandler.Animator.SetTrigger("Use");

            yield return new WaitForSeconds(healDelay);

            float healingAmount = Random.Range(m_HealingSettings.MinHealingAmount, m_HealingSettings.MaxHealingAmount);
            HealthEventData healEvent = new HealthEventData(healingAmount);

            Player.ChangeHealth.Try(healEvent);

            yield return new WaitForSeconds(delay);

            Player.Healing.ForceStop();

            m_IsHealing = false;
        }
    }

    #region Internal
    [Serializable]
    public class HealingSettings
    {
        [Range(0.1f, 10f)]
        public float UseAnimSpeed;

        [Range(0.1f, 2f)]
        public float HealTime;

        [Range(0f, 10f)]
        public float GiveHealthDelay;

        [Range(0f,100f)]
        public float MinHealingAmount;

        [Range(0f,100f)]
        public float MaxHealingAmount;
    }
    #endregion
}
