using System.Collections;
using UnityEngine;

namespace HQFPSWeapons
{
	public class EntityDeathHandler : LivingEntityComponent
	{
		[Header("Audio")]

		[SerializeField]
		private AudioSource m_AudioSource = null;

		[SerializeField]
		private SoundPlayer m_DeathAudio = null;

		[Header("Stuff To Disable On Death")]

		[SerializeField]
		private GameObject[] m_ObjectsToDisable = null;

		[SerializeField]
		private Behaviour[] m_BehavioursToDisable = null;

		[SerializeField]
		private Collider[] m_CollidersToDisable = null;

		[Header("Death Animation")]

		[SerializeField]
		[Tooltip("On death, you can either have a ragdoll, or an animation to play.")]
		private bool m_EnableDeathAnim = false;

		[SerializeField]
		private Animator m_Animator = null;

		[Header("Destroy Timer")]

		[SerializeField]
		[Clamp(0f, 1000f)]
		[Tooltip("")]
		private float m_DestroyTimer = 0f;


		private void Awake()
		{
			Entity.Health.AddChangeListener(OnChanged_Health);
		}

		private void OnChanged_Health(float health)
		{
			if(health == 0f)
				On_Death();
		}

		private void On_Death()
		{
			m_DeathAudio.Play(ItemSelection.Method.Random, m_AudioSource);

			if(m_EnableDeathAnim && m_Animator)
				m_Animator.SetTrigger("Die");

			foreach(var obj in m_ObjectsToDisable)
				obj.SetActive(false);

			foreach(var behaviour in m_BehavioursToDisable)
			{
				var animator = behaviour as Animator;
				if(animator != null)
					Destroy(animator);
				else
					behaviour.enabled = false;
			}

			foreach(var collider in m_CollidersToDisable)
				collider.enabled = false;

			Destroy(gameObject, m_DestroyTimer);

			Entity.Death.Send();
		}
	}
}
