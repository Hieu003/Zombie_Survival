using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	public class PlayerDeath : PlayerComponent
	{
		[SerializeField]
		private GameObject m_Camera = null;

		[BHeader("Audio")]

		[SerializeField]
		private AudioSource m_AudioSource = null;

		[SerializeField]
		private SoundPlayer m_DeathAudio = null;

		[BHeader("Stuff To Disable On Death")]

		[SerializeField]
		private GameObject[] m_ObjectsToDisable = null;

		[SerializeField]
		private Behaviour[] m_BehavioursToDisable = null;

		[SerializeField]
		private Collider[] m_CollidersToDisable = null;

		[BHeader("Player Head Hitbox")]

		[SerializeField]
		private Collider m_HeadCollider = null;

		[SerializeField]
		private Rigidbody m_HeadRigidbody = null;

		[BHeader("Respawn")]

		[SerializeField]
		private float m_RespawnDuration = 5f;

		[SerializeField]
		private bool m_RestartSceneOnRespawn = false;

		private Transform m_CameraStartParent;
		private Quaternion m_CameraStartRotation;
		private Vector3 m_CameraStartPosition;

		//Hitbox
		private Vector3 m_HeadHitboxStartPosition;
		private Quaternion m_HeadHitboxStartRotation;


		public void OnLoad()
		{
			Player.Health.AddChangeListener(OnChanged_Health);
		}

		private void Awake()
		{
			Player.Health.AddChangeListener(OnChanged_Health);

			//Camera set up
			m_CameraStartRotation = m_Camera.transform.localRotation;
			m_CameraStartPosition = m_Camera.transform.localPosition;
			m_CameraStartParent = m_Camera.transform.parent;

			//Hitbox set up
			m_HeadHitboxStartPosition = m_HeadCollider.transform.localPosition;
			m_HeadHitboxStartRotation = m_HeadCollider.transform.localRotation;
		}

		private void OnChanged_Health(float health)
		{
			if(health == 0f)
			{
				On_Death();

				m_Camera.transform.parent = m_HeadCollider.transform;
			}	
		}

		public void On_Death()
		{
			m_DeathAudio.Play(ItemSelection.Method.Random, m_AudioSource);

			//Disable
			foreach (var obj in m_ObjectsToDisable)
			{
				if (obj != null)
					obj.SetActive(false);
				else
					Debug.LogWarning("Check out PlayerDeath for missing references, an object reference was found null!", this);
			}

			foreach (var behaviour in m_BehavioursToDisable)
			{
				if (behaviour != null)
					behaviour.enabled = false;
				else
					Debug.LogWarning("Check out PlayerDeath for missing references, a behaviour reference was found null!", this);
			}

			foreach (var collider in m_CollidersToDisable)
			{
				if(collider != null)
					collider.enabled = false;
				else
					Debug.LogWarning("Check out PlayerDeath for missing references, a collider reference was found null!", this);
			}

			m_HeadCollider.transform.localPosition = m_HeadHitboxStartPosition;
			m_HeadCollider.transform.localRotation = m_HeadHitboxStartRotation;

			m_HeadCollider.isTrigger = false;
			m_HeadRigidbody.isKinematic = false;

			m_HeadRigidbody.AddForce(Player.Velocity.Get() * 0.5f, ForceMode.Force);
			m_HeadRigidbody.AddRelativeTorque(new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f) * 35, ForceMode.Force);

			PostProcessingManager.Instance.DoDeathAnim();
			Player.Death.Send();

			StartCoroutine(C_Respawn());
		}

		private IEnumerator C_Respawn()
		{
			yield return new WaitForSeconds(m_RespawnDuration);

			if (m_RestartSceneOnRespawn)
				GameManager.Instance.StartGame();
			else
			{
				GameManager.Instance.SetPlayerPosition();

				m_Camera.transform.parent = m_CameraStartParent;
				m_Camera.transform.localRotation = m_CameraStartRotation;
				m_Camera.transform.localPosition = m_CameraStartPosition;

				m_HeadCollider.isTrigger = true;
				m_HeadRigidbody.isKinematic = true;

				PostProcessingManager.Instance.RestoreDefaultProfile();

				foreach (var obj in m_ObjectsToDisable)
					obj.SetActive(true);

				foreach (var behaviour in m_BehavioursToDisable)
					behaviour.enabled = true;

				foreach (var collider in m_CollidersToDisable)
					collider.enabled = true;

				Player.MoveInput.Set(Vector2.zero);

				Player.Health.Set(100f);
				Player.Stamina.Set(100f);

				Player.Respawn.Send();
			}
		}
	}
}
