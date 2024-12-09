using UnityEngine;
using System.Collections;

namespace HQFPSWeapons
{
	/// <summary>
	/// 
	/// </summary>
	public class ItemPickup : InteractiveObject
	{
		/// <summary> </summary>
		public SaveableItem ItemInstance { get { return m_ItemInstance; } }
		public float InterractionProgress = 0f;
		public bool NeedToSwap { get; set; }
		public float SwapTime => m_SwapTime;
		public ItemContainerFlags TargetContainers => m_TargetContainers;

		[Space]

		[BHeader("Pick Up Method", order = 100)]

		[SerializeField]
		private PickUpMethod m_PickUpMethod = PickUpMethod.InteractionBased;

		[SerializeField]
		[ShowIf("m_PickUpMethod", (int)PickUpMethod.TriggerBased)]
		[Tooltip("The radius of the auto-created trigger.")]
		private float m_TriggerRadius = 0.5f;

		[SerializeField]
		[ShowIf("m_PickUpMethod", (int)PickUpMethod.InteractionBased)]
		private float m_SwapTime = 0f;

		[BHeader("Item", order = 100)]

		[SerializeField]
		[DatabaseItem]
		private string m_Item = string.Empty;

		[SerializeField]
		private int m_ItemCount = 1;

		[SerializeField]
		[Tooltip("In what container of the Player will the picked up item go")]
		private ItemContainerFlags m_TargetContainers = ItemContainerFlags.AmmoPouch;

		[SerializeField]
		private SoundPlayer m_CollideSounds = null;

		[SerializeField]
		private SoundPlayer m_PickupSounds = null;

		[SerializeField]
		private float m_PlayCollideSoundThresshold = 1f;

		[SerializeField]
		private LayerMask m_LayerMask = new LayerMask();

		[BHeader("Pick Up Message", order = 100)]

		[SerializeField]
		private Color m_BaseMessageColor = new Color(1f, 1f, 1f, 0.678f);

		[SerializeField]
		private Color m_ItemCountColor = new Color(0.976f, 0.6f, 0.129f, 1f);

		[SerializeField]
		private Color m_InventoryFullColor = Color.red;

		private SaveableItem m_ItemInstance;
		private string m_InitialInteractionText;
		private Rigidbody m_RigidB;

		private float m_NextTimePlayCollideSound;


		public void OnLoad()
		{
			SetInteractionText(m_ItemInstance);
		}

		public override void OnInteractionStart(Player player)
		{
			base.OnInteractionStart(player);

			if(NeedToSwap)
				InterractionProgress = 0;
			else
				OnPickUp(player);
		}

		public override void OnInteractionUpdate(Player player)
		{
			if (NeedToSwap)
			{
				InterractionProgress += Time.deltaTime;

				if (InterractionProgress > m_SwapTime)
					OnPickUp(player);
			}
		}

		public override void OnInteractionEnd(Player player)
		{
			base.OnInteractionEnd(player);

			InterractionProgress = 0;
		}

		public void EnablePickup(bool enable)
		{
			InteractionEnabled = enable;
		}

		public void SetItem(SaveableItem item)
		{
			m_ItemInstance = item;

			if(m_ItemInstance != null)
			{
				m_Item = m_ItemInstance.Name;
				SetInteractionText(m_ItemInstance);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (m_RigidB != null && m_LayerMask == (m_LayerMask | (1 << collision.collider.gameObject.layer)) && !collision.collider.isTrigger && Time.time > m_NextTimePlayCollideSound)
			{
				float collideVolume = Mathf.Clamp(collision.relativeVelocity.sqrMagnitude / 5f, 0.1f, 0.3f);

				if (m_RigidB.linearVelocity.sqrMagnitude > m_PlayCollideSoundThresshold)
					m_CollideSounds.PlayAtPosition(ItemSelection.Method.Random, transform.position, collideVolume);

				m_NextTimePlayCollideSound = Time.time + 0.5f;
			}
		}

		private void Awake()
		{
			m_InitialInteractionText = m_InteractionText;

			if(m_PickUpMethod != PickUpMethod.InteractionBased)
				InteractionEnabled = false;

			ItemData itemData;

			if(ItemDatabase.Default.TryGetItem(m_Item, out itemData))
				m_ItemInstance = new SaveableItem(itemData, m_ItemCount);

			// Create a trigger if the pickup method is set to WalkOver
			if(m_PickUpMethod == PickUpMethod.TriggerBased)
			{
				var sphereCol = gameObject.AddComponent<SphereCollider>();
				sphereCol.isTrigger = true;
				sphereCol.radius = m_TriggerRadius;
			}

			SetInteractionText(m_ItemInstance);

			if (GetComponent<Rigidbody>() != null)
				m_RigidB = GetComponent<Rigidbody>();

			m_NextTimePlayCollideSound = Time.time + 0.025f;
		}

		private void SetInteractionText(SaveableItem item)
		{
			if(item.CurrentStackSize < 2)
				m_InteractionText = string.Format(m_InitialInteractionText, item.Name.ToUpper());
			else
				m_InteractionText = string.Format(m_InitialInteractionText + " x " + item.CurrentStackSize, item.Name.ToUpper());
		}

		private void OnTriggerEnter(Collider col)
		{
			if(m_PickUpMethod != PickUpMethod.TriggerBased)
				return;

			var player = col.GetComponent<Player>();

			if(player != null)
				OnPickUp(player);
		}

		private void OnDrawGizmosSelected()
		{
			if(m_PickUpMethod == PickUpMethod.TriggerBased)
			{
				var prevColor = Gizmos.color;
				Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.2f);
				Gizmos.DrawSphere(transform.position, m_TriggerRadius);
				Gizmos.color = prevColor;
			}
		}

		private void OnValidate()
		{
			m_TriggerRadius = Mathf.Clamp(m_TriggerRadius, 0f, 2f);
		}

		private void OnPickUp(Player player)
		{
			if(m_ItemInstance != null)
			{
				bool destroy = false;
				bool swappedItems = false;

				if (player.Inventory.GetContainerWithFlags(m_TargetContainers).ContainerIsFull())
				{
					if (player.EquippedItem.Get() != null && player.SwapItems.Try(ItemInstance))
						swappedItems = true;
				}

				if(!swappedItems)
				{
					bool addedItem = player.Inventory.AddItem(m_ItemInstance, m_TargetContainers);

					// Item added to inventory
					if (addedItem)
					{
						if (m_ItemInstance.Data.StackSize > 1)
							MessageDisplayer.Instance.PushMessage(string.Format("Picked up <color={0}>{1}</color> x {2}", ColorUtils.ColorToHex(m_ItemCountColor), m_ItemInstance.Name, m_ItemInstance.CurrentStackSize), m_BaseMessageColor);
						else
							MessageDisplayer.Instance.PushMessage(string.Format("Picked up <color={0}>{1}</color>", ColorUtils.ColorToHex(m_ItemCountColor), m_ItemInstance.Name), m_BaseMessageColor);

						destroy = true;

						//Play pickup sound
						m_PickupSounds.Play2D(ItemSelection.Method.RandomExcludeLast);
					}
					// Item not added to inventory
					else
					{
						MessageDisplayer.Instance.PushMessage(string.Format("<color={0}>Inventory Full</color>", ColorUtils.ColorToHex(m_InventoryFullColor)), m_BaseMessageColor);
					}
				}
				// Item swapped
				else
				{
					destroy = true;
				}

				if(destroy)
					Destroy(gameObject);
			}
			else
			{
				Debug.LogError("Item Instance is null, can't pick up anything.");
				return;
			}
		}

		public IEnumerator C_DelayedDestroy(float lifeTime) 
		{
			yield return new WaitForSeconds(lifeTime);

			Destroy(gameObject);
		}

		// ------------------- Internal ------------------
		public enum PickUpMethod
		{
			TriggerBased,
			InteractionBased
		}
	}
}
