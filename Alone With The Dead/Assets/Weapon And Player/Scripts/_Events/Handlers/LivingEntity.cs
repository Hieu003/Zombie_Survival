using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// Base Class for any Entity
	/// </summary>
	public class LivingEntity : MonoBehaviour
	{
		public AudioSource AudioSource { get => m_AudioSource; }
		public Animator Animator { get => m_Animator; }

		/// <summary></summary>
		public readonly Value<float> Health = new Value<float>(100f);

		/// <summary> </summary>
		public readonly Attempt<HealthEventData> ChangeHealth = new Attempt<HealthEventData>();

		/// <summary> </summary>
		public readonly Value<bool> IsGrounded = new Value<bool>(true);

		/// <summary> </summary>
		public readonly Value<Vector3> Velocity = new Value<Vector3>(Vector3.zero);

		public Value<Vector3> LookDirection = new Value<Vector3>();

		/// <summary> </summary>
		public readonly Message<float> FallImpact = new Message<float>();

		/// <summary></summary>
		public readonly Message Death = new Message();

		/// <summary></summary>
		public readonly Message Respawn = new Message();

		public Hitbox[] Hitboxes;

		public Inventory Inventory { get { return m_Inventory; } }

		[BHeader("Main Components")]

		[SerializeField]
		private AudioSource m_AudioSource = null;

		[SerializeField]
		private Animator m_Animator = null;

		[SerializeField]
		private Inventory m_Inventory = null;


		private void Start()
		{
			Hitboxes = GetComponentsInChildren<Hitbox>();

			foreach (var component in GetComponentsInChildren<LivingEntityComponent>(true))
				component.OnEntityStart();
		}
	}
}