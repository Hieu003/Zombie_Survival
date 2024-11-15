using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// Handles the player input, and feeds it to the other components.
	/// </summary>
	public class PlayerInput_PC : PlayerComponent
	{
		private bool m_EquipmentCanBeUsed;
		private float m_NextTimeUseAutomatically;

		private void Update()
		{
			if (!Player.Pause.Active && Player.ViewLocked.Is(false))
			{
				// Movement.
				Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
				Player.MoveInput.Set(moveInput);

				// Look.
				Player.LookInput.Set(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));
				Player.WantsToInteract.Set(Input.GetButton("Interact"));

				// Jump.
				if (Input.GetButtonDown("Jump"))
					Player.Jump.TryStart();

				if (Input.GetButtonDown("Drop") && !Player.EquippedItem.Is(null) && !Player.Reload.Active && !Player.Healing.Active)
				{
					var item = Player.EquippedItem.Get();
					Player.Inventory.TryDropItem(item, true);
				}

				// Run.
				bool sprintButtonHeld = Input.GetButton("Run");
				bool canStartSprinting = Player.IsGrounded.Get() && Player.MoveInput.Get().y > 0f;

				if (!Player.Run.Active && sprintButtonHeld && canStartSprinting)
					Player.Run.TryStart();

				if (Player.Run.Active && !sprintButtonHeld)
					Player.Run.ForceStop();

				if (Input.GetButtonDown("Crouch"))
				{
					if (!Player.Crouch.Active)
						Player.Crouch.TryStart();
					else
						Player.Crouch.TryStop();
				}

				if (m_NextTimeUseAutomatically < Time.time)
					m_EquipmentCanBeUsed = false;

				// Change fire mode
				if (Input.GetButtonDown("ChangeFireMode"))
					Player.ChangeFireMode.Try();

				// Use item
				if (Input.GetButtonDown("UseEquipment") || m_EquipmentCanBeUsed)
				{
					bool usedSuccesfully = Player.UseOnce.Try();

					//Click Buffer
					if (Player.ActiveEquipmentItem.Val != null)
					{
						if (!usedSuccesfully && Player.ActiveEquipmentItem.Get().CurrentAmmoInfo.Val.CurrentInMagazine > 0 && Player.ActiveEquipmentItem.Get().UseClickBuffer)
						{
							m_EquipmentCanBeUsed = true;

							if (Input.GetButtonDown("UseEquipment"))
								m_NextTimeUseAutomatically = Time.time + 0.1f;
						}
					}
				}
				else if (Input.GetButton("UseEquipment"))
				{
					Player.UseContinuously.Try();
				}

				if (Input.GetButtonDown("ReloadEquipment"))
					Player.Reload.TryStart();

				// Aim
				var aimButtonPressed = Input.GetButton("Aim");

				if(!Player.Aim.Active && aimButtonPressed)
					Player.Aim.TryStart();
				else if(Player.Aim.Active && !aimButtonPressed)
					Player.Aim.ForceStop();

				//Heal
				if (Input.GetButton("Heal") && !aimButtonPressed)
					Player.Healing.TryStart();

				//Change Arms (Used for testing)
				if (Input.GetButtonDown("ChangeArms"))
					Player.ChangeArms.Try();

				//Suicide (Used for testing)
				if (Input.GetKeyDown(KeyCode.K))
				{
					HealthEventData damage = new HealthEventData(-1000f);
					Player.ChangeHealth.Try(damage);
				}
			}
			else
			{
				// Movement.
				Player.MoveInput.Set(Vector2.zero);

				// Look.
				Player.LookInput.Set(Vector2.zero);
			}

			var scrollWheelValue = Input.GetAxisRaw("Mouse ScrollWheel");
			Player.ScrollValue.Set(scrollWheelValue);
		}
	}
}
