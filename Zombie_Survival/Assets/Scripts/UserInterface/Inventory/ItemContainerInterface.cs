using UnityEngine;

namespace HQFPSWeapons.UserInterface
{
	public class ItemContainerInterface : ContainerInterface<ItemSlotInterface>
	{
		public ItemContainer ItemContainer
		{
			get
			{
				if(m_ItemContainer != null)
					return m_ItemContainer;
				else
				{
					Debug.LogError("There's no item container linked. Can't retrieve any!");

					return null;
				}
			}
		}

		[Header("Item Container")]

		[SerializeField]
		private bool m_IsPlayerContainer = true;

		[SerializeField]
		private string m_ContainerName = string.Empty;

		private ItemContainer m_ItemContainer = null;


		public void AttachToContainer(ItemContainer container)
		{
			bool generatedSlots = GenerateSlots(container.Count);

			if(generatedSlots)
			{
				m_ItemContainer = container;

				for(int i = 0;i < m_ItemContainer.Count;i ++)
					m_SlotInterfaces[i].LinkToSlot(m_ItemContainer[i]);
			}
		}

		public void DetachFromContainer()
		{
			if(m_ItemContainer == null)
				return;

			for(int i = 0;i < m_SlotInterfaces.Length;i++)
				m_SlotInterfaces[i].UnlinkFromSlot();
		}

		public override void OnAttachment()
		{
			if(m_IsPlayerContainer)
			{
				ItemContainer itemContainer = Player.Inventory.GetContainerWithName(m_ContainerName);

				if(itemContainer != null)
					AttachToContainer(itemContainer);
			}
		}
    }
}