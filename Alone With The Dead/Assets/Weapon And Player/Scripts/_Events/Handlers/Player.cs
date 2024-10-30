using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// Base Player Class
	/// </summary>
	public class Player : LivingEntity
	{
		public FirstPersonCamera Camera { get => m_Camera; }

		// Movement
		public Value<float> MovementSpeedFactor = new Value<float>(1f);
		public Value<float> MoveCycle = new Value<float>();
		public Message MoveCycleEnded = new Message();

		// Interaction
		public Value<RaycastData> RaycastData = new Value<RaycastData>(null);
		public Value<bool> WantsToInteract = new Value<bool>();

		/// <summary>Is there any object close to the camera? Eg. A wall</summary>
		public Value<bool> ObjectInProximity = new Value<bool>();

		public Activity Pause = new Activity();

		public Value<bool> ViewLocked = new Value<bool>();

		/// <summary></summary>
		public Value<float> Stamina = new Value<float>(100f);

		/// <summary></summary>
		public Value<Vector2> MoveInput = new Value<Vector2>(Vector2.zero);

		/// <summary></summary>
		public Value<Vector2> LookInput	= new Value<Vector2>(Vector2.zero);

		/// <summary></summary>
		public Value<float> ScrollValue = new Value<float>(0f);

		/// <summary></summary>
		public Value<SaveableItem> EquippedItem = new Value<SaveableItem>(null);

		/// <summary></summary>
		public Value<EquipmentItem> ActiveEquipmentItem = new Value<EquipmentItem>();

		/// <summary>
		/// <para>SavableItem - item to equip</para>
		/// <para>bool - do it instantly?</para>
		/// </summary>
		public Attempt<SaveableItem, bool> EquipItem = new Attempt<SaveableItem, bool>();

		/// <summary>
		/// <para>Destroy the held item.</para>
		/// float - destroy delay
		/// </summary>
		public Attempt<float> DestroyEquippedItem = new Attempt<float>();

		public Attempt<SaveableItem> SwapItems = new Attempt<SaveableItem>();
		public Attempt<SaveableItem> ItemIsSwappable = new Attempt<SaveableItem>();

		/// <summary></summary>
		public Attempt UseOnce = new Attempt();

		/// <summary></summary>
		public Attempt UseContinuously = new Attempt();

		/// <summary></summary>
		public Attempt ChangeFireMode = new Attempt();

		/// <summary></summary>
		public Attempt ChangeArms = new Attempt();

		/// <summary></summary>
		public Activity Walk = new Activity();

		/// <summary></summary>
		public Activity Run = new Activity();

		/// <summary></summary>
		public Activity Crouch = new Activity();

		/// <summary></summary>
		public Activity Jump = new Activity();

		/// <summary></summary>
		public Activity Aim = new Activity();

		/// <summary></summary>
		public Activity Reload = new Activity();

		/// <summary></summary>
		public Activity Healing = new Activity();

		/// <summary></summary>
		public Activity Swimming = new Activity();

		/// <summary></summary>
		public Activity OnLadder = new Activity();

		[SerializeField]
		private FirstPersonCamera m_Camera = null;
	}
}
