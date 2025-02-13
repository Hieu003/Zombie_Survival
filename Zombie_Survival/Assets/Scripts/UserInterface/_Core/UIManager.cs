using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HQFPSWeapons.UserInterface;
using UnityEngine.UI;


namespace HQFPSWeapons.UserInterface
{
    public class UIManager : MonoBehaviour
    {
        public readonly Value<bool> Dragging = new Value<bool>();
        public readonly Value<bool> DraggingItem = new Value<bool>();
        public readonly Message PointerDown = new Message();
        public readonly Activity OnConsoleOpened = new Activity();

        public Activity ItemWheel = new Activity();
        public Activity PauseMenu = new Activity();

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

        [SerializeField]
        private KeyCode m_PauseKey = KeyCode.Escape;

        [SerializeField]
        private GameObject m_PauseMenuPanel = null;

        private UserInterfaceBehaviour[] m_UIBehaviours;

        private void Awake()
        {
            PauseMenu.AddStartTryer(CanStartPauseMenu);
            PauseMenu.AddStopTryer(CanStopPauseMenu);
        }

        private bool CanStartPauseMenu()
        {
            // Add logic to determine if the PauseMenu can start
            return true;
        }

        private bool CanStopPauseMenu()
        {
            // Add logic to determine if the PauseMenu can stop
            return true;
        }

        public void AttachToPlayer(Player player)
        {
            if (!m_Canvas.isActiveAndEnabled)
                m_Canvas.gameObject.SetActive(true);

            if (m_UIBehaviours == null)
                m_UIBehaviours = GetComponentsInChildren<UserInterfaceBehaviour>(true);

            Player = player;

            for (int i = 0; i < m_UIBehaviours.Length; i++)
                m_UIBehaviours[i].OnAttachment();

            for (int i = 0; i < m_UIBehaviours.Length; i++)
                m_UIBehaviours[i].OnPostAttachment();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                PointerDown.Send();

            if (Input.GetKey(m_ItemWheelKey))
            {
                if (!ItemWheel.Active)
                {
                    if (ItemWheel.TryStart())
                        Player.ViewLocked.Set(true);
                }
            }
            else if (ItemWheel.Active && ItemWheel.TryStop())
                Player.ViewLocked.Set(false);

            if (Input.GetKeyDown(m_PauseKey))
            {
                if (!PauseMenu.Active)
                {
                    if (PauseMenu.TryStart())
                    {
                        m_PauseMenuPanel.SetActive(true);
                        Time.timeScale = 0f; // Pause the game
                        Player.ViewLocked.Set(true); // Lock player view
                        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
                        Cursor.visible = true; // Show the cursor
                    }
                }
                else if (PauseMenu.TryStop())
                {
                    m_PauseMenuPanel.SetActive(false);
                    Time.timeScale = 1f; // Resume the game
                    Player.ViewLocked.Set(false); // Unlock player view
                    Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
                    Cursor.visible = false; // Hide the cursor
                }
            }
        }

        public void ResumeGame()
        {
            if (PauseMenu.TryStop())
            {
                m_PauseMenuPanel.SetActive(false);
                Time.timeScale = 1f; // Resume the game
                Player.ViewLocked.Set(false); // Unlock player view
                Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
                Cursor.visible = false; // Hide the cursor
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f; // Ensure the game is not paused
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
        }

        public void QuitToMainMenu()
        {
            Time.timeScale = 1f; // Ensure the game is not paused
            SceneManager.LoadScene("MainMenu"); // Load the MainMenu scene
        }
    }
}
