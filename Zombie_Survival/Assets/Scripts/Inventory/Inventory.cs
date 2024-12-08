using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HQFPSWeapons
{
	public class Inventory : LivingEntityComponent
	{
		public Message ContainerChanged = new Message();

		public List<ItemContainer> Containers
		{
			get
			{ 
				if(m_AllContainers == null)
					InitiateContainers();

				return m_AllContainers;
			} 
		}

		[BHeader("Storage")]

		[SerializeField]
		private ContainerGenerator[] m_InitialContainers = null;

		[BHeader("Item Drop")]

		[SerializeField]
		private bool m_DropItemsOnDeath = true;

		[SerializeField]
		private SoundPlayer m_DropSounds = null;

		[Space]

		[SerializeField]
		private Vector3 m_DropOffset = new Vector3(0f, 0f, 0.8f);

		[SerializeField]
		private float m_DropAngularFactor = 150f;

		[SerializeField]
		private float m_DropSpeed = 8f;

		[SerializeField]
		private LayerMask m_WallsLayer = new LayerMask();

		private List<ItemContainer> m_SavableContainers;
		private List<ItemContainer> m_AllContainers;


		public void AddContainer(ItemContainer itemContainer, bool add)
		{
			if(add && !Containers.Contains(itemContainer))
			{
				Containers.Add(itemContainer);
				AddListeners(itemContainer, true);
			}
			else if(!add && Containers.Contains(itemContainer))
			{
				Containers.Remove(itemContainer);
				AddListeners(itemContainer, false);
			}
		}

		public bool HasContainerWithFlags(ItemContainerFlags flags)
		{
			for(int i = 0;i < Containers.Count;i ++)
				if(flags.HasFlag(Containers[i].Flag))
					return true;

			return false;
		}

		public ItemContainer GetContainerWithFlags(ItemContainerFlags flags)
		{
			for(int i = 0;i < Containers.Count;i ++)
				if(flags.HasFlag(Containers[i].Flag))
					return Containers[i];

			return null;
		}

		public ItemContainer GetContainerWithName(string name)
		{
			for(int i = 0;i < Containers.Count;i ++)
				if(Containers[i].Name == name)
					return Containers[i];

			return null;
		}

		public bool AddItem(SaveableItem item, ItemContainerFlags flags)
		{
			for(int i = 0;i < Containers.Count;i ++)
			{
				if(flags.HasFlag(Containers[i].Flag))
				{
					bool added = Containers[i].AddItem(item);
					if(added)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// </summary>
		public int AddItem(string itemName, int amountToAdd, ItemContainerFlags flags)
		{
			int addedInTotal = 0;

			for(int i = 0;i < Containers.Count;i ++)
			{
				if(flags.HasFlag(m_AllContainers[i].Flag))
				{
					int addedNow = Containers[i].AddItem(itemName, amountToAdd);
					addedInTotal += addedNow;
					if(addedNow == addedInTotal)
						return addedInTotal;
				}
			}

			return addedInTotal;
		}

		public bool RemoveItem(SaveableItem item)
		{
			for(int i = 0;i < Containers.Count;i ++)
			{
				if(m_AllContainers[i].RemoveItem(item))
					return true;
			}

			return false;
		}

		/// <summary>
		/// </summary>
		public int RemoveItems(string itemName, int amountToRemove, ItemContainerFlags flags)
		{
			int removedInTotal = 0;
			
			for(int i = 0;i < Containers.Count;i ++)
			{
				if(flags.HasFlag(Containers[i].Flag))
				{
					int removedNow = Containers[i].RemoveItem(itemName, amountToRemove);
					removedInTotal += removedNow;

					if (removedInTotal == amountToRemove)
						break;
				}
			}

			return removedInTotal;
		}

		/// <summary>
		/// Counts all the items with name itemName, from all containers.
		/// </summary>
		public int GetItemCount(string itemName)
		{
			int count = 0;
			for(int i = 0;i < Containers.Count;i ++)
				count += Containers[i].GetItemCount(itemName);

			return count;
		}

		public ItemSlot GetItemSlot(SaveableItem item)
		{
			foreach (var container in m_SavableContainers)
			{
				foreach (ItemSlot slot in container)
				{
					if (slot.Item == item)
						return slot;
				}
			}

			return null;
		}

		public bool TryDropItem(SaveableItem item, bool tryRemoveFromContainers = false)
		{
			bool canBeDropped = item != null && item.Data.WorldObject != null && (!tryRemoveFromContainers || RemoveItem(item));
		
			if(canBeDropped)
			{
				Player player = Entity as Player;

				float crouchHeightDrop = 1f;

				if (player != null) 
					crouchHeightDrop = 1f;

				StartCoroutine(C_Drop(item, crouchHeightDrop));

				return true;
			}

			return false;
		}

		private IEnumerator C_Drop(SaveableItem item, float heightDropMultiplier)
		{
			if (item == null)
				yield return null;

			bool nearWall = false;

			Vector3 dropPosition;
			Quaternion dropRotation;

			if (Physics.Raycast(transform.position, transform.InverseTransformDirection(Vector3.forward), m_DropOffset.z, m_WallsLayer))
			{
				dropPosition = transform.position + transform.TransformVector(new Vector3(0f, m_DropOffset.y * heightDropMultiplier, -0.2f));
				dropRotation = Quaternion.LookRotation(Entity.LookDirection.Get());
				nearWall = true;
			}
			else
			{
				dropPosition = transform.position + transform.TransformVector(new Vector3(m_DropOffset.x, m_DropOffset.y * heightDropMultiplier, m_DropOffset.z));
				dropRotation = Random.rotationUniform;
			}

			GameObject droppedItem = Instantiate(item.Data.WorldObject, dropPosition, dropRotation) as GameObject;

			var rigidbody = droppedItem.GetComponent<Rigidbody>();
			var collider = droppedItem.GetComponent<Collider>();

			if (rigidbody != null)
			{
				Physics.IgnoreCollision(GetComponent<Collider>(), collider);

				rigidbody.isKinematic = false;

				if (rigidbody != null && !nearWall)
				{
					rigidbody.AddTorque(Random.rotation.eulerAngles * m_DropAngularFactor);
					rigidbody.AddForce(Entity.LookDirection.Get() * m_DropSpeed, ForceMode.VelocityChange);
				}
			}

			m_DropSounds.Play2D(ItemSelection.Method.RandomExcludeLast);

			var pickup = droppedItem.GetComponent<ItemPickup>();

			if (pickup != null)
				pickup.SetItem(item);
		}

		private void Awake()
		{
			if(ItemDatabase.Default == null)
			{
				Debug.LogError("No ItemDatabase found, this storage component will be disabled!", this);
				enabled = false;

				return;
			}
	
			Entity.Death.AddListener(OnEntityDeath);
			InitiateContainers();
		}

		private void InitiateContainers()
		{
			m_SavableContainers = new List<ItemContainer>();

			for (int i = 0; i < m_InitialContainers.Length; i++)
			{
				var container = m_InitialContainers[i].GenerateContainer(transform);
				m_SavableContainers.Add(container);

				AddListeners(container, true);
			}

			m_AllContainers = new List<ItemContainer>(m_SavableContainers);
		}

		private void AddListeners(ItemContainer container, bool add)
		{
			if (add)
				container.Changed.AddListener(OnContainerChanged);
			else
				container.Changed.RemoveListener(OnContainerChanged);
		}

		private void OnContainerChanged(ItemSlot slot)
		{
			try
			{
				ContainerChanged.Send();
			}
			catch {
			};
		}

		private void OnEntityDeath()
		{
			if(m_DropItemsOnDeath)
			{
				for(int i = 0; i < m_AllContainers.Count; i ++)
				{
					for(int j = 0; j < m_AllContainers[i].Slots.Length; j ++)
					{
						var slot = m_AllContainers[i].Slots[j];

						if(slot.Item)
						{
							TryDropItem(slot.Item);
							slot.SetItem(null);
						}
					}
				}
			}
		}
	}

	public class NestedContainer
	{
		public ItemContainer Container;
		public ItemSlot Slot;
	}
}
