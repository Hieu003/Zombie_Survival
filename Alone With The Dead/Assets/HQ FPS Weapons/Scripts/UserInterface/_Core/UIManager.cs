using UnityEngine;

namespace HQFPSWeapons.UserInterface
{
	public class UIManager : MonoBehaviour
	{
		public readonly Value<bool> Dragging = new Value<bool>();
		public readonly Value<bool> DraggingItem = new Value<bool>();
		public readonly Message PointerDown = new Message();
		public readonly Activity OnConsoleOpened = new Activity();

		public Activity ItemWheel = new Activity();

		public Player Player { get; private set; }

		/// <summary>The main Canvas that's used for the GUI elements.</summary>
		public Canvas Canvas { get { return m_Canvas; } }

		public Font Font { get { return m_Font; } }

		[BHeader("SETUP", true)]

		[SerializeField]
		private Canvas m_Canvas = null;

		[SerializeField]
		private Font m_Font = null;

		[SerializeField]
		private KeyCode m_ItemWheelKey = KeyCode.Q;

		private UserInterfaceBehaviour[] m_UIBehaviours;


		public void AttachToPlayer(Player player)
		{
			if (!m_Canvas.isActiveAndEnabled)
				m_Canvas.gameObject.SetActive(true);

			if (m_UIBehaviours == null)
				m_UIBehaviours = GetComponentsInChildren<UserInterfaceBehaviour>(true);

			Player = player;

			for(int i = 0;i < m_UIBehaviours.Length;i ++)
				m_UIBehaviours[i].OnAttachment();

			for(int i = 0;i < m_UIBehaviours.Length;i ++)
				m_UIBehaviours[i].OnPostAttachment();
		}

		private void Update()
		{
			if(Input.GetMouseButtonDown(0))
				PointerDown.Send();

			if(Input.GetKey(m_ItemWheelKey))
			{
				if(!ItemWheel.Active)
				{
					if(ItemWheel.TryStart())
						Player.ViewLocked.Set(true);
				}
			}
			else if(ItemWheel.Active && ItemWheel.TryStop())
				Player.ViewLocked.Set(false);
		}
	}
}
