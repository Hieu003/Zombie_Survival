using UnityEngine;

namespace HQFPSWeapons
{
    public class PlayerMovement : PlayerComponent
    {
        public bool IsGrounded { get => m_Controller.isGrounded; }
        public Vector3 Velocity { get => m_Controller.velocity; }
        public Vector3 SurfaceNormal { get; private set; }
        public float SlopeLimit { get => m_Controller.slopeLimit; }
        public float DefaultHeight { get; private set; }

        [SerializeField]
        private CharacterController m_Controller = null;

        [SerializeField]
        private LayerMask m_ObstacleCheckMask = ~0;

        [BHeader("Core Movement...")]

        [SerializeField]
        [Range(0f, 20f)]
        private float m_Acceleration = 5f;

        [SerializeField]
        [Range(0f, 20f)]
        private float m_Damping = 8f;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_AirborneControl = 0.15f;

        [SerializeField]
        [Range(0f, 3f)]
        private float m_StepLength = 1.2f;

        [SerializeField]
        [Range(0f, 10f)]
        private float m_ForwardSpeed = 2.5f;

        [SerializeField]
        [Range(0f, 10f)]
        private float m_BackSpeed = 2.5f;

        [SerializeField]
        [Range(0f, 10f)]
        private float m_SideSpeed = 2.5f;

        [SerializeField]
        private AnimationCurve m_SlopeSpeedMult = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

        [SerializeField]
        private float m_AntiBumpFactor = 1f;

        [BHeader("Running...")]

        [SerializeField]
        private bool m_EnableRunning = true;

        [SerializeField]
        [Range(1f, 10f)]
        private float m_RunSpeed = 4.5f;

        [SerializeField]
        [Range(0f, 3f)]
        private float m_RunStepLength = 1.9f;

        [BHeader("Jumping...")]

        [SerializeField]
        private bool m_EnableJumping = true;

        [SerializeField]
        [Range(0f, 3f)]
        private float m_JumpHeight = 1f;

        [SerializeField]
        [Range(0f, 1.5f)]
        private float m_JumpTimer = 0.3f;

        [BHeader("Crouching...")]

        [SerializeField]
        private bool m_EnableCrouching = false;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_CrouchSpeedMult = 0.7f;

        [SerializeField]
        [Range(0f, 3f)]
        private float m_CrouchStepLength = 0.8f;

        [SerializeField]
        [Range(0f, 2f)]
        private float m_CrouchHeight = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_CrouchDuration = 0.3f;

        [BHeader("Sliding...")]

        [SerializeField]
        private bool m_EnableSliding = false;

        [SerializeField]
        [Range(20f, 90f)]
        private float m_SlideTreeshold = 32f;

        [SerializeField]
        [Range(0f, 50f)]
        private float m_SlideSpeed = 15f;

        [BHeader("Misc...")]

        [SerializeField]
        [Range(0f, 100f)]
        private float m_Gravity = 20f;

        private float m_UncrouchedHeight = 0f;

        private Vector3 m_DesiredVelocityLocal;
        private Vector3 m_SlideVelocity;

        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private float m_LastLandTime;

        private float m_NextTimeCanCrouch;

        private float m_DistMovedSinceLastCycleEnded;
        private float m_CurrentStepLength;


        private void Awake()
        {
            DefaultHeight = m_Controller.height;

            RaycastHit hitInfo;

            if(Physics.Raycast(transform.position + transform.up, -transform.up, out hitInfo, 3f, ~0, QueryTriggerInteraction.Ignore))
                transform.position = hitInfo.point + Vector3.up * 0.08f;
        }

        private void Start()
        {
            Player.IsGrounded.AddChangeListener(OnGroundingStateChanged);
            Player.Run.AddStartTryer(TryRun);
            Player.Jump.AddStartTryer(TryJump);
            Player.Crouch.AddStartTryer(TryCrouch);
            Player.Crouch.AddStopTryer(TryUncrouch);
            Player.Death.AddListener(OnDeath);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            Vector3 translation = Vector3.zero;

            if (IsGrounded)
            {
                translation = transform.TransformVector(m_DesiredVelocityLocal) * deltaTime;

                if (!Player.Jump.Active)
                    translation.y = -m_AntiBumpFactor;
            }
            else
                translation = transform.TransformVector(m_DesiredVelocityLocal * deltaTime);

            m_CollisionFlags = m_Controller.Move(translation);

            if ((m_CollisionFlags & CollisionFlags.Below) == CollisionFlags.Below && !m_PreviouslyGrounded)
            {
                bool wasJumping = Player.Jump.Active;

                if (Player.Jump.Active)
                    Player.Jump.ForceStop();

                Player.FallImpact.Send(Mathf.Abs(m_DesiredVelocityLocal.y));

                m_LastLandTime = Time.time;

                if (wasJumping)
                    m_DesiredVelocityLocal = Vector3.ClampMagnitude(m_DesiredVelocityLocal, 1f);
            }

            Vector3 targetVelocity = CalcTargetVelocity(Player.MoveInput.Get());

            if (!IsGrounded)
                UpdateAirborneMovement(deltaTime, targetVelocity, ref m_DesiredVelocityLocal);
            else if (!Player.Jump.Active)
                UpdateGroundedMovement(deltaTime, targetVelocity, ref m_DesiredVelocityLocal);

            Player.IsGrounded.Set(IsGrounded);
            Player.Velocity.Set(Velocity);

            m_PreviouslyGrounded = IsGrounded;
        }

        private void UpdateGroundedMovement(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
        {
            // Make sure to lower the speed when moving on steep surfaces.
            float surfaceAngle = Vector3.Angle(Vector3.up, SurfaceNormal);
            targetVelocity *= m_SlopeSpeedMult.Evaluate(surfaceAngle / SlopeLimit);

            // Calculate the rate at which the current speed should increase / decrease. 
            // If the player doesn't press any movement button, use the "m_Damping" value, otherwise use "m_Acceleration".
            float targetAccel = targetVelocity.sqrMagnitude > 0f ? m_Acceleration : m_Damping;

            velocity = Vector3.Lerp(velocity, targetVelocity, targetAccel * deltaTime);

            // If we're moving and not running, start the "Walk" activity.
            if (!Player.Walk.Active && targetVelocity.sqrMagnitude > 0.05f && !Player.Run.Active && !Player.Crouch.Active)
                Player.Walk.ForceStart();
            // If we're running, or not moving, stop the "Walk" activity.
            else if (Player.Walk.Active && (targetVelocity.sqrMagnitude < 0.05f || Player.Run.Active || Player.Crouch.Active))
                Player.Walk.ForceStop();

            if (Player.Run.Active)
            {
                bool wantsToMoveBackwards = Player.MoveInput.Get().y < 0f;
                bool runShouldStop = wantsToMoveBackwards || targetVelocity.sqrMagnitude == 0f || Player.Stamina.Is(0f);

                if (runShouldStop)
                    Player.Run.ForceStop();
            }

            if (m_EnableSliding)
            {
                // Sliding...
                if (surfaceAngle > m_SlideTreeshold && Player.MoveInput.Get().sqrMagnitude == 0f)
                {
                    Vector3 slideDirection = (SurfaceNormal + Vector3.down);
                    m_SlideVelocity += slideDirection * m_SlideSpeed * deltaTime;
                }
                else
                    m_SlideVelocity = Vector3.Lerp(m_SlideVelocity, Vector3.zero, deltaTime * 10f);

                velocity += transform.InverseTransformVector(m_SlideVelocity);
            }

            // Advance step
            m_DistMovedSinceLastCycleEnded += m_DesiredVelocityLocal.magnitude * deltaTime;

            // Which step length should be used?
            float targetStepLength = m_StepLength;

            if (Player.Crouch.Active)
                targetStepLength = m_CrouchStepLength;
            else if (Player.Run.Active)
                targetStepLength = m_RunStepLength;

            m_CurrentStepLength = Mathf.MoveTowards(m_CurrentStepLength, targetStepLength, deltaTime * 0.6f);

            // If the step cycle is complete, reset it, and send a notification.
            if (m_DistMovedSinceLastCycleEnded > m_CurrentStepLength)
            {
                m_DistMovedSinceLastCycleEnded -= m_CurrentStepLength;
                Player.MoveCycleEnded.Send();
            }

            Player.MoveCycle.Set(m_DistMovedSinceLastCycleEnded / m_CurrentStepLength);
        }

        private void UpdateAirborneMovement(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
        {
            if (m_PreviouslyGrounded && !Player.Jump.Active)
                velocity.y = 0f;

            // Modify the current velocity by taking into account how well we can change direction when not grounded (see "m_AirControl" tooltip).
            velocity += targetVelocity * m_Acceleration * m_AirborneControl * deltaTime;

            // Apply gravity.
            velocity.y -= m_Gravity * deltaTime;
        }

        private bool TryRun()
        {
            if (!m_EnableRunning || Player.Stamina.Get() < 15f)
                return false;

            bool wantsToMoveBack = Player.MoveInput.Get().y < 0f;

            return Player.IsGrounded.Get() && !wantsToMoveBack && !Player.Crouch.Active && !Player.Aim.Active;
        }

        private bool TryJump()
        {
            // If crouched, stop crouching first
            if (Player.Crouch.Active)
            {
                Player.Crouch.TryStop();
                return false;
            }

            bool canJump = m_EnableJumping &&
                IsGrounded &&
                !Player.Crouch.Active &&
                Time.time > m_LastLandTime + m_JumpTimer;

            if (!canJump)
                return false;
            float jumpSpeed = Mathf.Sqrt(2 * m_Gravity * m_JumpHeight);
            m_DesiredVelocityLocal = new Vector3(m_DesiredVelocityLocal.x, jumpSpeed, m_DesiredVelocityLocal.z);

            return true;
        }

        private bool TryCrouch()
        {
            bool canCrouch =
                m_EnableCrouching &&
                (Time.time > m_NextTimeCanCrouch || m_NextTimeCanCrouch == 0f) &&
                Player.IsGrounded.Get() &&
                !Player.Run.Active;

            if (canCrouch)
            {
                SetHeight(m_CrouchHeight);
                m_NextTimeCanCrouch = Time.time + m_CrouchDuration;
            }

            return canCrouch;
        }

        private bool TryUncrouch()
        {
            bool obstacleAbove = DoCollisionCheck(true, Mathf.Abs(m_CrouchHeight - m_UncrouchedHeight));
            bool canStopCrouching = Time.time > m_NextTimeCanCrouch && !obstacleAbove;

            if (canStopCrouching)
            {
                SetHeight(DefaultHeight);
                m_NextTimeCanCrouch = Time.time;
            }

            return canStopCrouching;
        }

        private void OnGroundingStateChanged(bool isGrounded)
        {
            if (!isGrounded)
            {
                Player.Walk.ForceStop();
                Player.Run.ForceStop();
            }
        }

        private Vector3 CalcTargetVelocity(Vector2 moveInput)
        {
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);

            bool wantsToMove = moveInput.sqrMagnitude > 0f;

            // Calculate the direction (relative to the us), in which the player wants to move.
            Vector3 targetDirection = (wantsToMove ? new Vector3(moveInput.x, 0f, moveInput.y) : m_DesiredVelocityLocal.normalized);

            float desiredSpeed = 0f;

            if (wantsToMove)
            {
                // Set the default speed.
                desiredSpeed = m_ForwardSpeed;

                // If the player wants to move sideways...
                if (Mathf.Abs(moveInput.x) > 0f)
                    desiredSpeed = m_SideSpeed;

                // If the player wants to move backwards...
                if (moveInput.y < 0f)
                    desiredSpeed = m_BackSpeed;

                // If we're currently running...
                if (Player.Run.Active)
                {
                    // If the player wants to move forward or sideways, apply the run speed multiplier.
                    if (desiredSpeed == m_ForwardSpeed || desiredSpeed == m_SideSpeed)
                        desiredSpeed = m_RunSpeed;
                }

                // If we're crouching...
                if (Player.Crouch.Active)
                    desiredSpeed *= m_CrouchSpeedMult;
            }

            return targetDirection * (desiredSpeed * Player.MovementSpeedFactor.Val);
        }

        private bool DoCollisionCheck(bool checkAbove, float maxDistance, out RaycastHit hitInfo)
        {
            Vector3 rayOrigin = transform.position + (checkAbove ? Vector3.up * m_Controller.height : Vector3.zero);
            Vector3 rayDirection = checkAbove ? Vector3.up : Vector3.down;

            return Physics.SphereCast(new Ray(rayOrigin, rayDirection), m_Controller.radius, out hitInfo, maxDistance, m_ObstacleCheckMask, QueryTriggerInteraction.Ignore);
        }

        private bool DoCollisionCheck(bool checkAbove, float maxDistance)
        {
            Vector3 rayOrigin = transform.position + (checkAbove ? Vector3.up * m_Controller.height : Vector3.zero);
            Vector3 rayDirection = checkAbove ? Vector3.up : Vector3.down;

            return Physics.SphereCast(new Ray(rayOrigin, rayDirection), m_Controller.radius, maxDistance, m_ObstacleCheckMask, QueryTriggerInteraction.Ignore);
        }

        private void SetHeight(float height)
        {
            m_Controller.height = height;
            m_Controller.center = Vector3.up * height * 0.5f;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            SurfaceNormal = hit.normal;
        }

        private void OnDeath()
        {
            m_DesiredVelocityLocal = Vector3.zero;
        }
    }
}