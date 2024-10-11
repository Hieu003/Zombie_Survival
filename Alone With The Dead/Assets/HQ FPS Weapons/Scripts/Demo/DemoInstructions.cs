using UnityEngine;
using UnityEngine.UI;

namespace HQFPSWeapons.Demo
{
	public class DemoInstructions : MonoBehaviour
	{
		[SerializeField]
		private bool m_InstructionsEnabledOnStart = false;

		[Space(3f)]

		[SerializeField]
		private Text m_MessageToggleText = null;

		[SerializeField]
		private GameObject m_InstructionsObject = null;

		private bool m_InstructionsEnabled;


		private void Awake()
		{
			m_InstructionsEnabled = m_InstructionsEnabledOnStart;
			Refresh();
		}

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.F12))
			{
				m_InstructionsEnabled = !m_InstructionsEnabled;
				Refresh();
			}
		}

		private void Refresh()
		{
			m_InstructionsObject.gameObject.SetActive(m_InstructionsEnabled);
			m_MessageToggleText.text = "Press F12 to " + (m_InstructionsEnabled ? "hide" : "show") + " the instructions.";
		}
	}
}