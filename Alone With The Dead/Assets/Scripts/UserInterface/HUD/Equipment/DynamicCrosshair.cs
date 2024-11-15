using UnityEngine;
using UnityEngine.UI;

namespace HQFPSWeapons.UserInterface
{
	/// <summary>
	/// 
	/// </summary>
	public class DynamicCrosshair : MonoBehaviour
	{
		public float Distance { get { return m_Distance; } }

		[SerializeField]
		[Clamp(0f, 256f)]
		private float m_Distance = 32f;

		[Header("Crosshair Parts")]

		[SerializeField]
		private Image m_Left = null;

		[SerializeField]
		private Image m_Right = null;

		[SerializeField]
		private Image m_Down = null;

		[SerializeField]
		private Image m_Up = null;


		public void SetActive(bool active)
		{
			m_Left.enabled = m_Right.enabled = m_Down.enabled = m_Up.enabled = active;
		}

		public void SetDistance(float distance)
		{
			m_Left.rectTransform.anchoredPosition = new Vector2(-distance, 0f);
			m_Right.rectTransform.anchoredPosition = new Vector2(distance, 0f);
			m_Down.rectTransform.anchoredPosition = new Vector2(0f, -distance);
			m_Up.rectTransform.anchoredPosition = new Vector2(0f, distance);

			m_Distance = distance;
		}

		public void SetColor(Color color)
		{
			m_Left.color = m_Right.color = m_Down.color = m_Up.color = color;
		}

		private void OnValidate()
		{
			if(!Application.isPlaying)
				SetDistance(m_Distance);
		}
	}
}
