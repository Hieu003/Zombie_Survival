using UnityEngine;

namespace HQFPSWeapons
{
	public class HealsManager : PlayerComponent
	{
		[SerializeField]
		private string m_HealsContainerName = "Backpack";

		[SerializeField]
		private bool m_HealWhileRunning = false;

		[SerializeField]
		private bool m_HealWithMaxHealth = false;


		private void Awake()
        {
			Player.Healing.AddStartTryer(TryStart_Healing);
			Player.Healing.AddStopListener(OnStop_Healing);
		}

        private bool TryStart_Healing()
		{
			if (((Player.Run.Active && !m_HealWhileRunning) ||
				(Player.Health.Get() >= 100 && !m_HealWithMaxHealth)) ||
				Player.Healing.Active)
				return false;

			bool startedHealing = false;

			SaveableItem healingItem = TryGetHealingItem();

			if (healingItem != null)
			{
				startedHealing = true;

				if (Player.Reload.Active)
					Player.Reload.ForceStop();

				if (Player.Aim.Active)
					Player.Aim.ForceStop();

				Player.EquipItem.Try(healingItem, false);
			}

			return startedHealing;
		}

		private void OnStop_Healing()
		{
			Player.Inventory.RemoveItems(Player.EquippedItem.Val.Data.Name, 1, ItemContainerFlags.Storage);
			Player.EquipItem.Try(Player.EquippedItem.GetPreviousValue(), false);
		}

		private SaveableItem TryGetHealingItem()
		{
			if (!Player.Inventory.HasContainerWithFlags(ItemContainerFlags.Storage))
				return null;

			ItemContainer container = Player.Inventory.GetContainerWithName(m_HealsContainerName);

			foreach (var slot in container.Slots)
			{
				if (slot.HasItem)
					return slot.Item;
			}

			return null;
		}
	}
}
