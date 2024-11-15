using UnityEngine;

namespace HQFPSWeapons.UserInterface
{
	public class Panel : MonoBehaviour 
	{
        public Message<bool> VisibilityChanged = new Message<bool>();

		public bool IsVisible { get; private set; }
		public bool IsInteractable { get; private set; }
        public bool RequiresManualOpening { get { return m_RequiresManualOpening; } }

		[BHeader("GENERAL", true)]

		[SerializeField]
		private bool m_ShowOnAwake = false;

		[SerializeField]
		private bool m_RequiresManualOpening = false;

		[Space]

		[SerializeField]
		private CanvasGroup m_CanvasGroup = null;

		[SerializeField]
		private Animator m_Animator = null;

		[BHeader("Animation Speed...")]

		[SerializeField]
		[Range(0f, 2f)]
		private float m_DefaultSpeed = 1f;

		[SerializeField]
		[Range(0f, 2f)]
		private float m_ShowSpeed = 1f;

		[SerializeField]
		[Range(0f, 2f)]
		private float m_HideSpeed = 1f;

		[SerializeField]
		[Range(0f, 2f)]
		private float m_RefreshSpeed = 1f;

		[BHeader("Audio...")]

		[SerializeField]
		private AudioClip m_ShowSound = null;

		[SerializeField]
		private float m_ShowSoundVolume = 0.5f;


		public void TryShow(bool show)
		{
			if(IsVisible == show)
				return;

			if(m_Animator != null)
				m_Animator.SetTrigger(show ? "Show" : "Hide");

			SetIsInteractable(show);

			IsVisible = show;

            VisibilityChanged.Send(IsVisible);

			if(show && m_ShowSound != null)
				AudioUtils.Instance.Play2D(m_ShowSound, m_ShowSoundVolume);
		}

        public bool TryRefresh()
        {
            if(m_Animator != null)
            {
                m_Animator.SetTrigger("Refresh");
                return true;
            }

            return false;
        }

        public bool TryEnableDefaultMode()
        {
            if(m_Animator != null)
                m_Animator.Play("Hide", 0, 1f);

            SetIsInteractable(false);

            IsVisible = false;

            return true;
        }

        public void SetIsInteractable(bool isInteractable)
		{
			if(m_CanvasGroup != null)
				m_CanvasGroup.blocksRaycasts = isInteractable;

			IsInteractable = isInteractable;
		}
			
		private void Awake()
		{
			SetAnimationSpeeds();

            if(!m_ShowOnAwake)
                TryEnableDefaultMode();
            else
                TryShow(true);
		}

		private void SetAnimationSpeeds()
		{
			if(Application.isPlaying && m_Animator != null)
			{
				m_Animator.SetFloat("Default Speed", m_DefaultSpeed);
				m_Animator.SetFloat("Show Speed", m_ShowSpeed);
				m_Animator.SetFloat("Hide Speed", m_HideSpeed);
				m_Animator.SetFloat("Refresh Speed", m_RefreshSpeed);
			}
		}
	}
}