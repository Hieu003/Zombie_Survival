using UnityEngine;

namespace HQFPSWeapons.UserInterface
{
	public class UserInterfaceBehaviour : MonoBehaviour 
	{
		public UIManager Manager
		{
			get 
			{
				if(!m_Manager)
					m_Manager = GetComponentInChildren<UIManager>();
				if(!m_Manager)
					m_Manager = GetComponentInParent<UIManager>();

				return m_Manager;
			}
		}

		public Player Player { get { return Manager != null ? Manager.Player : null; } }

		public Inventory PlayerStorage { get { return Player != null ? Player.Inventory : null; } }

		private UIManager m_Manager;

		public virtual void OnAttachment() {  }

		public virtual void OnPostAttachment() {  }
	}
}
