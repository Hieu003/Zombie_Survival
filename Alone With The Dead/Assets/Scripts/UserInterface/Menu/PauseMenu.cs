using UnityEngine;

namespace HQFPSWeapons.UserInterface
{
	public class PauseMenu : UserInterfaceBehaviour
	{
		[SerializeField]
		private Panel m_Panel = null;

		[SerializeField]
		private Panel m_MapSelectionPanel = null;

		[SerializeField]
		private bool m_UseKeyToPause = true;

		[SerializeField]
		[ShowIf("m_UseKeyToPause", true)]
		private KeyCode m_PauseKey = KeyCode.Escape;


		public void Pause()
		{
			Player.Pause.ForceStart();

			Time.timeScale = 0f;
			m_Panel.TryShow(true);

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public void LoadScene(int index)
		{
			Resume();

			GameManager.Instance.StartGame(index);
		}

		public void Resume()
		{
			Player.Pause.ForceStop();

			Time.timeScale = 1f;
			m_Panel.TryShow(false);

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			m_MapSelectionPanel.TryShow(false);
		}

		public void ToggleMapSelection()
		{
			m_MapSelectionPanel.TryShow(!m_MapSelectionPanel.IsVisible);
		}

		public void Quit()
		{
			GameManager.Instance.Quit();
		}

		private void Update()
		{
			if(m_UseKeyToPause && Input.GetKeyDown(m_PauseKey))
			{
				if (!Player.Pause.Active)
				{
					Pause();
				}
				else
					Resume();
			}
		}
	}
}