using System;
using System.Collections;
using UnityEngine;

namespace HQFPSWeapons
{
    public class Unarmed : EquipmentItem
    {
        #region Internal
        [Serializable]
        private class GeneralUnarmedSettings
        {
            [Tooltip("Right arm layer int, from the Unarmed animator")]
            public int RightArmLayerInt = 1;

            public bool AlwaysShowArms = false;

            [ShowIf("AlwaysShowArms", false, 10f)]
            [Tooltip("How much time the arms will be on the screen if the Player punches")]
            public float ArmsShowDuration = 3f;

            [Space]

            public float RunAnimSpeed = 1f;

            public float RunAnimStartTime = 0.5f;
        }

        [Serializable]
        private class SwimmingModule { }

        [Serializable]
        private class LadderClimbingModule { }

        [Serializable]
        private class PunchingModule
        {
            public LayerMask HitMask = new LayerMask();

            public SurfaceEffects ImpactEffect = SurfaceEffects.Slash;

            public DamageType DamageType = DamageType.Hit;

            [Tooltip("Useful for limiting the number of hits you can do in a period of time.")]
            public float Cooldown = 1f;

            [Space(3)]

            [BHeader("Sphere Cast", order = 100)]

            [Tooltip("How far can this weapon hit stuff?")]
            public float CastDistance = 1.5f;

            public float CastDelay = 0.4f;

            public float CastRadius = 0.2f;

            [Space(3)]

            [BHeader("Impact", order = 100)]

            public float DamagePerHit = 5f;

            public float ImpactForcePerHit = 10f;

            [Space(3)]

            [BHeader("Audio", order = 100)]

            public DelayedSound PunchAudio = null;

            [Space(3)]

            [BHeader("Animation", order = 100)]

            public int AnimationAmount = 2;

            public float AnimationSpeed = 1f;

            [Space(3)]

            [BHeader("Camera Forces", order = 100)]

            public DelayedCameraForce PunchCamForce = null;

            public DelayedCameraForce ImpactCamForce = null;
        }
        #endregion

        [SerializeField]
        [Group]
        private GeneralUnarmedSettings m_GeneralUnarmedSettings = null;

        [SerializeField]
        [Group]
        private PunchingModule m_PunchingSettings = null;

        private float m_NextTimeToHideArms = 1f;
        private bool m_ArmsAreVisible;

        private int m_LastPunchIndex = 0;


        public override void Wield(SaveableItem item)
        {
            if (m_GeneralUnarmedSettings.AlwaysShowArms || Player.Run.Active)
                ChangeArmsVisibility(true);

            m_NextTimeCanUse = Time.time + m_PunchingSettings.Cooldown;
        }

        public override void Unwield()
        {
            if (m_ArmsAreVisible)
                m_EHandler.Animator.SetTrigger("Hide");

            m_EHandler.Animator.SetBool("IsAirborne", false);

            ChangeArmsVisibility(false);
        }

        public override bool TryUseOnce(Camera camera)
        {
            if (m_NextTimeCanUse < Time.time && Player.IsGrounded.Val == true)
            {
                m_NextTimeCanUse = Time.time + m_PunchingSettings.Cooldown;
                m_NextTimeToHideArms = Time.time + m_GeneralUnarmedSettings.ArmsShowDuration;

                //If the arms are not on screen play the show animation
                if (!m_ArmsAreVisible)
                    ChangeArmsVisibility(true);
                else
                {
                    Punch();

                    return true;
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            Player.Run.RemoveStartListener(OnStartRunning);
            Player.Run.RemoveStopListener(OnStopRunning);
            Player.Jump.RemoveStartListener(OnStartJumping);

            Player.IsGrounded.RemoveChangeListener(OnStartFalling);

            //TODO: Add Swimming
            //Player.Swimming.RemoveStartListener(OnStartSwimming);
            //Player.Swimming.RemoveStopListener(OnStopSwimming);
        }

        private void Start()
        {
            Player.Run.AddStartListener(OnStartRunning);
            Player.Run.AddStopListener(OnStopRunning);

            Player.Jump.AddStartListener(OnStartJumping);
            Player.IsGrounded.AddChangeListener(OnStartFalling);

            //TODO: Add Swimming
            //Player.Swimming.AddStartListener(OnStartSwimming);
            //Player.Swimming.AddStopListener(OnStopSwimming);

            m_EHandler.Animator.SetFloat("RunSpeed", m_GeneralUnarmedSettings.RunAnimSpeed);

            if (m_GeneralUnarmedSettings.AlwaysShowArms)
            {
                m_EHandler.Animator.SetBool("ArmsAreVisible", true);
                m_ArmsAreVisible = true;
            }
        }

        private void OnValidate()
        {
            if (m_EHandler != null && m_EHandler.Animator != null)
                m_EHandler.Animator.SetFloat("RunSpeed", m_GeneralUnarmedSettings.RunAnimSpeed);
        }

        private void Punch()
        {
            int punchIndex = (m_LastPunchIndex >= m_PunchingSettings.AnimationAmount - 1) ? 0 : m_LastPunchIndex + 1;

            //Play Animations
            m_EHandler.Animator.SetFloat("PunchIndex", punchIndex);
            m_EHandler.Animator.SetTrigger("Punch");

            //Play Camera Forces
            m_EHandler.PlayCameraForce(m_PunchingSettings.PunchCamForce);

            //Play Audio
            m_EHandler.PlayDelayedSound(m_PunchingSettings.PunchAudio);

            m_LastPunchIndex = punchIndex;

            StartCoroutine(C_SphereCastDelayed(Player.Camera.UnityCamera));
        }

        private void OnStartFalling(bool isGrounded)
        {
            if (!PlayerIsUnarmed() || m_EHandler.Animator == null)
                return;

            if (isGrounded)
            {
                m_EHandler.Animator.SetBool("IsAirborne", false);
                m_EHandler.Animator.SetBool("Jumping", false);

                m_NextTimeCanUse = Time.time + m_PunchingSettings.Cooldown;
            }
            else
            {
                m_EHandler.Animator.SetTrigger("Falling");
                m_EHandler.Animator.SetBool("IsAirborne", true);
            }
        }

        private void OnStartRunning() 
        {
            m_EHandler.Animator.SetBool("IsRunning", true);
        }

        private void OnStopRunning()
        {
            m_EHandler.Animator.SetBool("IsRunning", false);

            if (!PlayerIsUnarmed())
            {
                m_NextTimeCanUse = Time.time + m_PunchingSettings.Cooldown;

                ChangeArmsVisibility(false);
            }
        }

        private void OnStartJumping()
        {
            if (!PlayerIsUnarmed())
                return;

            m_EHandler.Animator.SetBool("IsAirborne", true);
            m_EHandler.Animator.SetBool("Jumping", true);
        }

        private void Update()
        {
            if (!PlayerIsUnarmed())
                return;

            if (!m_GeneralUnarmedSettings.AlwaysShowArms && m_NextTimeToHideArms < Time.time && m_ArmsAreVisible)
            {
                ChangeArmsVisibility(false);
                m_EHandler.Animator.SetTrigger("Hide");
            }
        }

        private void ChangeArmsVisibility(bool show) 
        {
            m_ArmsAreVisible = show;
            m_EHandler.Animator.SetBool("ArmsAreVisible", show);
        }

        protected virtual IDamageable SphereCast(Camera camera)
        {
            IDamageable damageable = null;
            RaycastHit hitInfo;

            if (Physics.SphereCast(camera.transform.position, m_PunchingSettings.CastRadius, camera.transform.forward, out hitInfo, m_PunchingSettings.CastDistance, m_PunchingSettings.HitMask, QueryTriggerInteraction.Ignore))
            {
                SurfaceManager.SpawnEffect(hitInfo, m_PunchingSettings.ImpactEffect, 1f);

                // Apply an impact impulse
                if (hitInfo.rigidbody != null)
                    hitInfo.rigidbody.AddForceAtPosition(camera.transform.forward * m_PunchingSettings.ImpactForcePerHit, hitInfo.point, ForceMode.Impulse);

                var damageData = new HealthEventData(-m_PunchingSettings.DamagePerHit, m_PunchingSettings.DamageType, hitInfo.point, camera.transform.forward, m_PunchingSettings.ImpactForcePerHit, Player);

                // Do damage
                damageable = hitInfo.collider.GetComponent<IDamageable>();

                if (damageable != null)
                    damageable.TakeDamage(damageData);

                // Camera force
                m_EHandler.PlayCameraForce(m_PunchingSettings.ImpactCamForce);
            }

            return damageable;
        }

        private bool PlayerIsUnarmed() 
        {
            return Player.EquippedItem.Val == null;
        }

        private IEnumerator C_SphereCastDelayed(Camera camera)
        {
            yield return new WaitForSeconds(m_PunchingSettings.CastDelay);

            SphereCast(camera);
        }
    }
}
