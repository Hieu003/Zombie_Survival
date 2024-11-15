using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// 
	/// </summary>
	public class MouseLook : PlayerComponent
	{
		public float SensitivityFactor { get; set; }

		public Vector2 LookAngles
		{
			get => m_LookAngles;

			set
			{
				m_LookAngles = value;
			}
		}

		public Vector2 LastMovement { get; private set; }

		[BHeader("General", true)]

		[SerializeField] 
		[Tooltip("The camera root which will be rotated up & down (on the X axis).")]
		private Transform m_LookRoot = null;

		[SerializeField]
		private Transform m_PlayerRoot = null;

		[SerializeField]
		[Tooltip("The up & down rotation will be inverted, if checked.")]
		private bool m_Invert = false;

		[BHeader("Motion")]

		[SerializeField] 
		[Tooltip("The higher it is, the faster the camera will rotate.")]
		private float m_Sensitivity = 5f;

		[SerializeField]
		private float m_AimSensitivity = 2.5f;

		[SerializeField]
		private float m_RollAngle = 10f;

		[SerializeField]
		private float m_RollSpeed = 3f;

		[BHeader("Rotation Limits")]

		[SerializeField]
		private Vector2 m_DefaultLookLimits = new Vector2(-60f, 90f);

		private float m_CurrentRollAngle;
		private Vector2 m_LookAngles;

		private bool m_Loaded;


		public void MoveCamera(float verticalMove, float horizontalMove)
		{
			LookAngles += new Vector2(verticalMove, horizontalMove);
		}

		public void OnLoad()
		{
			m_Loaded = true;
		}

		private void Awake()
		{
			SensitivityFactor = 1f;
		}

		private void Start()
		{
			if(!m_LookRoot)
			{
				Debug.LogErrorFormat(this, "Assign the look root in the inspector!", name);
				enabled = false;
			}

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			if(!m_Loaded)
				m_LookAngles = new Vector2(transform.localEulerAngles.x, m_PlayerRoot.localEulerAngles.y);
		}

		private void LateUpdate()
		{
			Vector2 prevLookAngles = m_LookAngles;

			if (Player.ViewLocked.Is(false) && Player.Health.Get() > 0f)
			{
				LookAround();
			}

			LastMovement = m_LookAngles - prevLookAngles;
		}

		/// <summary>
		/// Rotates the camera and character and creates a sensation of looking around.
		/// </summary>
		private void LookAround()
		{
			var sensitivity = Player.Aim.Active ? m_AimSensitivity : m_Sensitivity;
			sensitivity *= SensitivityFactor;

			m_LookAngles.x += Player.LookInput.Get().y * sensitivity * (m_Invert ? 1f : -1f);
			m_LookAngles.y += Player.LookInput.Get().x * sensitivity;

			m_LookAngles.x = ClampAngle(m_LookAngles.x, m_DefaultLookLimits.x, m_DefaultLookLimits.y);

			m_CurrentRollAngle = Mathf.Lerp(m_CurrentRollAngle, Player.LookInput.Get().x * m_RollAngle, Time.deltaTime * m_RollSpeed);

			// Apply the current up & down rotation to the look root.
			m_LookRoot.localRotation = Quaternion.Euler(m_LookAngles.x, 0f, 0f);

			m_PlayerRoot.localRotation = Quaternion.Euler(0f, m_LookAngles.y, 0f);

			Entity.LookDirection.Set(m_LookRoot.forward);
		}

		/// <summary>
		/// Clamps the given angle between min and max degrees.
		/// </summary>
		private float ClampAngle(float angle, float min, float max) 
		{
			if(angle > 360f)
				angle -= 360f;
			else if(angle < -360f)
				angle += 360f;
			
			return Mathf.Clamp(angle, min, max);
		}
	}
}
