using UnityEngine;
using UnityEngine.UI;

namespace HQFPSWeapons.UserInterface
{
	public class CrosshairManager : HUD_DisplayerBehaviour
	{
		[BHeader("GENERAL", true)]

		[SerializeField]
		private CanvasGroup m_CanvasGroup = null;

		[SerializeField]
		private Image m_StaticCrosshair = null;

		[SerializeField]
		private DynamicCrosshair m_DynamicCrosshair = null;

		[Space]

		[SerializeField]
		private bool m_HideWhenRunning = true;

		[SerializeField]
		private bool m_HideWhenReloading = true;

		[SerializeField]
		private bool m_ApplyUsePunch = true;

		[Space]

		[BHeader("Crosshairs...", order = 100)]

		[SerializeField]
		private bool m_EnableDefaultCrosshair = false;

		[SerializeField]
		[ShowIf("m_EnableDefaultCrosshair", true)]
		private CrosshairData m_DefaultCrosshair = null;

		[SerializeField]
		private CrosshairData m_RunCrosshair = null;

		[SerializeField]
		[Group]
		private CrosshairData[] m_CustomCrosshairs = null;

		private const float CROSSHAIR_UNIT_SCALE = 42f;

		private CrosshairData m_CurrentCrosshair;
		private CrosshairData m_PreviousCrosshair;


		public override void OnPostAttachment()
		{
			Player.Pause.AddStartListener(OnPauseStart);
			Player.Pause.AddStopListener(OnPauseEnd);

			Player.EquippedItem.AddChangeListener(OnChanged_HeldItem);

			Player.UseOnce.AddListener(ApplyUsePunch);
			Player.UseContinuously.AddListener(ApplyUsePunch);

			Player.Run.AddStartListener(OnStart_Run);
			Player.Run.AddStopListener(OnStop_Run);

			Player.Aim.AddStartListener(OnStart_Aim);
			Player.Aim.AddStopListener(OnStop_Aim);

			Player.Reload.AddStartListener(OnReloadStart);
			Player.Reload.AddStopListener(OnReloadEnd);

			if(Player.EquippedItem.Get())
				OnChanged_HeldItem(Player.EquippedItem.Get());
		}

		private void Update()
		{
			if(m_CurrentCrosshair != null)
			{
				var crosshairType = m_CurrentCrosshair.Type;

				var raycastInfo = Player.RaycastData.Get();
				bool onEntity = false;

				if(raycastInfo != null && raycastInfo.Collider != null && raycastInfo.Collider.GetComponent<Hitbox>())
					onEntity = true;

				if(crosshairType == CrosshairType.Dynamic && m_DynamicCrosshair != null)
				{
					m_DynamicCrosshair.SetColor(onEntity ? m_CurrentCrosshair.OnEntityColor : m_CurrentCrosshair.NormalColor);

					float distance = m_CurrentCrosshair.DynamicCrosshair.IdleScale * CROSSHAIR_UNIT_SCALE;

					if(Player.Crouch.Active)
						distance = m_CurrentCrosshair.DynamicCrosshair.CrouchScale * CROSSHAIR_UNIT_SCALE;
					else if(Player.Walk.Active)
						distance = m_CurrentCrosshair.DynamicCrosshair.WalkScale * CROSSHAIR_UNIT_SCALE;
					else if(Player.Run.Active)
						distance = m_CurrentCrosshair.DynamicCrosshair.RunScale * CROSSHAIR_UNIT_SCALE;
					else if(Player.Jump.Active)
						distance = m_CurrentCrosshair.DynamicCrosshair.JumpScale * CROSSHAIR_UNIT_SCALE;

					if(Player.Aim.Active)
						distance *= m_CurrentCrosshair.DynamicCrosshair.AimMultiplier;

					m_DynamicCrosshair.SetDistance(Mathf.MoveTowards(m_DynamicCrosshair.Distance, distance, Time.deltaTime * CROSSHAIR_UNIT_SCALE * m_CurrentCrosshair.DynamicCrosshair.m_MoveSpeed));
				}
				else if(crosshairType == CrosshairType.Static && m_StaticCrosshair != null)
					m_StaticCrosshair.color = onEntity ? m_CurrentCrosshair.OnEntityColor : m_CurrentCrosshair.NormalColor;
			}
		}

		private void SetActive(bool active, CrosshairData crosshair)
		{
			if(crosshair.Type == CrosshairType.Static)
			{
				m_StaticCrosshair.enabled = active;

				if(active)
				{
					m_StaticCrosshair.color = new Color(m_StaticCrosshair.color.r, m_StaticCrosshair.color.g, m_StaticCrosshair.color.b, 0);

					m_StaticCrosshair.sprite = crosshair.StaticCrosshair.Sprite;
					m_StaticCrosshair.rectTransform.sizeDelta = crosshair.StaticCrosshair.Size;
				}
			}
			else
				m_DynamicCrosshair.SetActive(active);
		}

		private void ApplyUsePunch()
		{
			if(!m_ApplyUsePunch || m_CurrentCrosshair == null)
				return;

			var crosshairType = m_CurrentCrosshair.Type;

			if(crosshairType == CrosshairType.Dynamic && m_DynamicCrosshair != null)
			{
				float distance = m_CurrentCrosshair.DynamicCrosshair.IdleScale * CROSSHAIR_UNIT_SCALE;

				if(Player.Crouch.Active)
					distance = m_CurrentCrosshair.DynamicCrosshair.CrouchScale * CROSSHAIR_UNIT_SCALE;
				else if(Player.Walk.Active)
					distance = m_CurrentCrosshair.DynamicCrosshair.WalkScale * CROSSHAIR_UNIT_SCALE;
				else if(Player.Run.Active)
					distance = m_CurrentCrosshair.DynamicCrosshair.RunScale * CROSSHAIR_UNIT_SCALE;
				else if(Player.Jump.Active)
					distance = m_CurrentCrosshair.DynamicCrosshair.JumpScale * CROSSHAIR_UNIT_SCALE;

				m_DynamicCrosshair.SetDistance(Mathf.Lerp(m_DynamicCrosshair.Distance, distance * m_CurrentCrosshair.DynamicCrosshair.m_PunchSize, Time.deltaTime * 20f));
			}
		}

		private void OnPauseStart()
		{
			m_CanvasGroup.alpha = 0f;
			m_CanvasGroup.blocksRaycasts = false;
		}

		private void OnPauseEnd()
		{
			m_CanvasGroup.alpha = 1f;
			m_CanvasGroup.blocksRaycasts = true;
		}

		private void OnChanged_HeldItem(SaveableItem item)
		{
			if(m_CurrentCrosshair != null)
			{
				SetActive(false, m_CurrentCrosshair);
				m_CurrentCrosshair = null;
			}

			try
			{
				if (item.Data != null)
				{
					for (int i = 0; i < m_CustomCrosshairs.Length; i++)
						if (m_CustomCrosshairs[i].ItemName == item.Data.Name)
						{
							m_CurrentCrosshair = m_CustomCrosshairs[i];
							SetActive(true, m_CurrentCrosshair);

							return;
						}
				}
			}
			catch {; }

			if (m_EnableDefaultCrosshair)
			{
				m_CurrentCrosshair = m_DefaultCrosshair;
				SetActive(true, m_CurrentCrosshair);
			}
		}

		private void OnStart_Run()
		{
			if (m_HideWhenRunning)
				SetActive(false, m_CurrentCrosshair);

			else if (m_RunCrosshair != null && Player.ActiveEquipmentItem.Val != null)
			{
				if (m_CurrentCrosshair != null)
				{
					SetActive(false, m_CurrentCrosshair);
				}

				m_PreviousCrosshair = m_CurrentCrosshair;

				m_CurrentCrosshair = m_RunCrosshair;
				SetActive(true, m_CurrentCrosshair);

				m_StaticCrosshair.color = new Color(m_RunCrosshair.NormalColor.r, m_RunCrosshair.NormalColor.g, m_RunCrosshair.NormalColor.b, 0);
			}
		}

		private void OnStop_Run()
		{
			if (m_CurrentCrosshair == null)
				return;

			if (m_RunCrosshair != null && Player.ActiveEquipmentItem.Val != null)
			{
				SetActive(false, m_CurrentCrosshair);

				m_CurrentCrosshair = m_PreviousCrosshair;
			}

			if (((Player.Aim.Active && !m_CurrentCrosshair.ShowWhenAiming) || !Player.Aim.Active) && m_CurrentCrosshair != null)
				SetActive(true, m_CurrentCrosshair);
		}

		private void OnStart_Aim()
		{
			if(m_CurrentCrosshair != null && !m_CurrentCrosshair.ShowWhenAiming && ((Player.Run.Active && !!m_CurrentCrosshair.ShowWhenAiming) || !Player.Run.Active))
				SetActive(false, m_CurrentCrosshair);
		}

		private void OnStop_Aim()
		{
			if(m_CurrentCrosshair != null)
				SetActive(true, m_CurrentCrosshair);
		}

		private void OnReloadStart()
		{
			if(m_CurrentCrosshair != null && m_HideWhenReloading)
				SetActive(false, m_CurrentCrosshair);
		}

		private void OnReloadEnd()
		{
			if(m_CurrentCrosshair != null && m_HideWhenReloading)
				SetActive(true, m_CurrentCrosshair);
		}
	}
}
