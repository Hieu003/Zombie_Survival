using UnityEngine;
using System;
using System.Collections;

namespace HQFPSWeapons
{
    /// <summary>
    /// A component which can be attached to a weapon (E.g. Gun / LauncherWeapon)
    /// It handles all of the recoil for the attached weapon from camera to weapon model recoil.
    /// </summary>
    [RequireComponent(typeof(ProjectileBasedWeapon))]
    public class WeaponRecoil : FPEquipmentComponent
    {
        #region Internal
        [Serializable]
        public class WeaponRecoilModule : CloneableObject<WeaponRecoilModule>
        {
            public AnimationCurve RecoilOverTime = null;

            [Space]

            public Spring.Data PositionSpring = Spring.Data.Default;

            public Spring.Data RotationSpring = Spring.Data.Default;

            [Space]

            [Group]
            public RecoilForce ShootForce = new RecoilForce();

            [Group]
            public RecoilForce AimShootForce = new RecoilForce();

            [Group]
            public RecoilForce DryFireForce = new RecoilForce();

            [Group]
            public RecoilForce ChangeFireModeForce = new RecoilForce();
        }

        [Serializable]
        public class CameraRecoilModule : CloneableObject<CameraRecoilModule>
        {
            [SerializeField]
            [Range(0f, 2f)]
            public float AimMultiplier = 0.75f;

            [BHeader("Controllable recoil", order = 100)]

            public float RecoilPatternMultiplier = 1f;

            [Tooltip("Controllable - vertical(x) & horizontal(y) recoil, add as many values as the mag size of the attached weapon")]
            public Vector2[] RecoilPattern;

            public Easings.Function RecoilControlEasing = Easings.Function.QuadraticEaseInOut;

            public float RecoilControlDelay = 1f;
            public float RecoilControlDuration = 3f;

            [Space(3f)]

            [BHeader("Non-Controllable recoil", order = 100)]

            public Spring.Data SpringData = Spring.Data.Default;

            [Tooltip("Non-Controlable - Use/Shoot cam force")]
            public RecoilForce ShootForce = new RecoilForce();

            [Tooltip("Non-Controlable - Use/Shoot cam shake")]
            public CameraShakeSettings ShootShake = null;
        }
        #endregion

        [SerializeField]
        [Group]
        private WeaponRecoilModule m_WeaponModelRecoil = null;

        [SerializeField]
        [Group]
        private CameraRecoilModule m_CameraRecoil = null;

        private ProjectileBasedWeapon m_AttachedWeapon;

        private bool m_ChangedSpring;
        private bool m_AdditiveRecoilActive;
        private bool m_RecoilControlActive;
        private Easer m_RecoilControlLerper;

        private float m_RecoilStartTime;

        private Vector2 m_RecoilToAdd;
        private Vector2 m_RecoilFrameRemove;

        
        private void Start()
        {
            m_EHandler.UsingItem.AddStartListener(StartRecoil);
            m_EHandler.UsingItem.AddStopListener(StopRecoil);
            m_EHandler.ItemUsed.AddListener(AddImpulseRecoil);

            Player.ChangeFireMode.AddListener(ChangeFireModeForce);

            Player.Camera.AdjustRecoilSettings(m_CameraRecoil.SpringData, m_CameraRecoil.SpringData);
            m_RecoilControlLerper = new Easer(m_CameraRecoil.RecoilControlEasing, m_CameraRecoil.RecoilControlDuration);

            m_AttachedWeapon = m_EHandler.CurrentItem as ProjectileBasedWeapon;
        }

        private void OnDestroy()
        {
            m_EHandler.UsingItem.RemoveStartListener(StartRecoil);
            m_EHandler.UsingItem.RemoveStopListener(StopRecoil);
            m_EHandler.ItemUsed.RemoveListener(AddImpulseRecoil);
            Player.ChangeFireMode.RemoveListener(ChangeFireModeForce);
        }

        private void OnValidate()
        {
            if (m_CameraRecoil != null)
            {
                if (Player != null && Player.Camera != null)
                    Player.Camera.AdjustRecoilSettings(m_CameraRecoil.SpringData, m_CameraRecoil.SpringData);

                m_RecoilControlLerper = new Easer(m_CameraRecoil.RecoilControlEasing, m_CameraRecoil.RecoilControlDuration);
            }
        }

        //Non-Controllable/Impulse recoil
        private void AddImpulseRecoil(bool successfully, bool continuously) 
        {
            if (successfully)
            {
                if ((m_AttachedWeapon.ContinuouslyUsedTimes == 1 || !m_ChangedSpring) && !Player.Aim.Active)
                {
                    m_EHandler.PhysicsHandler.PositionSpring.Adjust(m_WeaponModelRecoil.PositionSpring);
                    m_EHandler.PhysicsHandler.RotationSpring.Adjust(m_WeaponModelRecoil.RotationSpring);

                    m_ChangedSpring = true;
                }

                //Apply a random recoil force to the visual model
                float recoilMultiplier = m_WeaponModelRecoil.RecoilOverTime.Evaluate(m_AttachedWeapon.ContinuouslyUsedTimes / (float)m_AttachedWeapon.TryGetMagazineSize());

                if(Player.Aim.Active)
                    m_WeaponModelRecoil.AimShootForce.PlayRecoilForce(recoilMultiplier, m_EHandler.PhysicsHandler.RotationSpring, m_EHandler.PhysicsHandler.PositionSpring);
                else
                    m_WeaponModelRecoil.ShootForce.PlayRecoilForce(recoilMultiplier, m_EHandler.PhysicsHandler.RotationSpring, m_EHandler.PhysicsHandler.PositionSpring);

                //Apply a recoil force to the camera
                Player.Camera.ApplyRecoil(m_CameraRecoil.ShootForce, Player.Aim.Active ? m_CameraRecoil.AimMultiplier : 1f);

                //Apply a shake force to the camera
                if (m_CameraRecoil.ShootShake.PositionAmplitude != Vector3.zero && m_CameraRecoil.ShootShake.RotationAmplitude != Vector3.zero)
                    Player.Camera.DoShake(m_CameraRecoil.ShootShake, 1f);
            }
            else
            {
                //Add dry fire force to the weapon model
                if (!continuously)
                {
                    if ((m_AttachedWeapon.CurrentAmmoInfo.Val.CurrentInMagazine == 0 && !m_EHandler.UsingItem.Active)
                        || (m_AttachedWeapon.SelectedFireMode == (int)ProjectileBasedWeapon.FireMode.Safety))
                        m_WeaponModelRecoil.DryFireForce.PlayRecoilForce(1f, m_EHandler.PhysicsHandler.RotationSpring, m_EHandler.PhysicsHandler.PositionSpring);
                }
            }
        }

        //Controllable/Additive recoil
        private void StartRecoil() 
        {
            m_RecoilStartTime = Time.time;
            m_AdditiveRecoilActive = true;

            m_RecoilToAdd = Vector2.zero;

            StopCoroutine(C_StartRecoilControl());
        }
        
        private void Update() 
        {
            if (m_CameraRecoil.RecoilPattern.Length == 0)
                return;

            if (m_AdditiveRecoilActive)
            {
                //Additive Recoil
                int recoilControlIndex = Mathf.Clamp(m_AttachedWeapon.ContinuouslyUsedTimes - 1, 0, m_CameraRecoil.RecoilPattern.Length - 1);

                Vector2 recoilThisFrame = new Vector2(
                    m_CameraRecoil.RecoilPattern[recoilControlIndex].x * m_CameraRecoil.RecoilPatternMultiplier * Time.deltaTime,
                    m_CameraRecoil.RecoilPattern[recoilControlIndex].y * m_CameraRecoil.RecoilPatternMultiplier * Time.deltaTime);

                if (Player.Aim.Active)
                    recoilThisFrame *= m_CameraRecoil.AimMultiplier;

                m_EHandler.EquipmentManager.MouseLook.MoveCamera(recoilThisFrame.x, recoilThisFrame.y);

                Vector2 lastMovement = -m_EHandler.EquipmentManager.MouseLook.LastMovement;

                m_RecoilToAdd -= recoilThisFrame;

                if (m_RecoilToAdd.x != 0f && Mathf.Sign(lastMovement.x) != Mathf.Sign(m_RecoilToAdd.x))
                {
                    m_RecoilToAdd.x = Mathf.Clamp(m_RecoilToAdd.x + lastMovement.x, 0f, Mathf.Infinity);
                }

                if (m_RecoilToAdd.y != 0f && Mathf.Sign(lastMovement.y) != Mathf.Sign(m_RecoilToAdd.y))
                    m_RecoilToAdd.y = Mathf.Clamp(m_RecoilToAdd.y + lastMovement.y, 0f, Mathf.Infinity);
            }
            else if (m_RecoilControlActive)
            {
                //Recoil Control
                Vector2 lastMovement = m_EHandler.EquipmentManager.MouseLook.LastMovement;

                if (m_RecoilToAdd.x != 0f && Mathf.Sign(lastMovement.x) != Mathf.Sign(m_RecoilToAdd.x))
                {
                    m_RecoilToAdd.x = Mathf.Clamp(m_RecoilToAdd.x + lastMovement.x, 0f, Mathf.Infinity);
                }

                if (m_RecoilToAdd.y != 0f && Mathf.Sign(lastMovement.y) != Mathf.Sign(m_RecoilToAdd.y))
                    m_RecoilToAdd.y = Mathf.Clamp(m_RecoilToAdd.y + lastMovement.y, 0f, Mathf.Infinity);

                m_RecoilControlLerper.Update(Time.deltaTime);

                Vector2 prevRecoil = m_RecoilToAdd;
                RemoveRecoil(ref m_RecoilToAdd, Time.deltaTime * m_RecoilFrameRemove);

                Vector2 recoilThisFrame = m_RecoilToAdd - prevRecoil;

                recoilThisFrame.y = 0f;
                m_EHandler.EquipmentManager.MouseLook.LookAngles -= recoilThisFrame;

                if (m_RecoilToAdd.sqrMagnitude == 0f)
                {
                    m_RecoilControlActive = false;
                }
            }
        }

        private void StopRecoil() 
        {
            m_AdditiveRecoilActive = false;
            m_ChangedSpring = false;

            //Start the recoil control after a certain amount of time
            StartCoroutine(C_StartRecoilControl());
        }

        private void RemoveRecoil(ref Vector2 recoil, Vector2 amount)
        {
            float signX = Mathf.Sign(recoil.x);
            float signY = Mathf.Sign(recoil.y);

            recoil.x -= recoil.x * amount.x;
            recoil.y -= recoil.y * amount.y;

            if (Mathf.Sign(recoil.x) != signX)
                recoil.x = 0f;

            if (Mathf.Sign(recoil.y) != signY)
                recoil.y = 0f;
        }

        private void ChangeFireModeForce() 
        {
            m_WeaponModelRecoil.ChangeFireModeForce.PlayRecoilForce(1f, m_EHandler.PhysicsHandler.RotationSpring, m_EHandler.PhysicsHandler.PositionSpring);
        }

        private IEnumerator C_StartRecoilControl()
        {
            yield return new WaitForSeconds(m_CameraRecoil.RecoilControlDelay);

            m_RecoilControlLerper.Reset();

            m_RecoilFrameRemove = m_RecoilToAdd * (1f / (m_CameraRecoil.RecoilControlDuration * (Time.time - m_RecoilStartTime)));

            m_RecoilControlActive = true;
        }
    }
}
