using UnityEngine;
using UnityEngine.Events;

namespace HQFPSWeapons
{
	/// <summary>
	/// Base class for interactive objects (eg. buttons, item pickups).
	/// Has numerous raycast and interaction callbacks (overridable).
	/// </summary>
	public class InteractiveObject : MonoBehaviour
	{
		public bool InteractionEnabled { get { return m_InteractionEnabled; } set { m_InteractionEnabled = value; } }
		public string InteractionText { get { return m_InteractionText; } }

		[BHeader("Interaction", true)]

		[SerializeField]
		private bool m_InteractionEnabled = true;

		[SerializeField]
		[Multiline]
		protected string m_InteractionText = string.Empty;

		[Space]

		[SerializeField]
		private SoundPlayer m_RaycastStartAudio = null;

		[SerializeField]
		private SoundPlayer m_RaycastEndAudio = null;

		[SerializeField]
		private SoundPlayer m_InteractionStartAudio = null;

		[SerializeField]
		private SoundPlayer m_InteractionEndAudio = null;

		public UnityEvent m_InteractEvent = null;


		/// <summary>
		/// Called when a player starts looking at the object.
		/// </summary>
		public virtual void OnRaycastStart(Player player) 
		{
			m_RaycastStartAudio.Play2D(); 
		}

		/// <summary>
		/// Called while a player is looking at the object.
		/// </summary>
		public virtual void OnRaycastUpdate(Player player) {  }

		/// <summary>
		/// Called when a player stops looking at the object.
		/// </summary>
		public virtual void OnRaycastEnd(Player player) 
		{
			m_RaycastEndAudio.Play2D(); 
		}

		/// <summary>
		/// Called when a player starts interacting with the object.
		/// </summary>
		public virtual void OnInteractionStart(Player player) 
		{
			m_InteractionStartAudio.Play2D();
			m_InteractEvent.Invoke();
		}

		/// <summary>
		/// Called while a player is interacting with the object.
		/// </summary>
		public virtual void OnInteractionUpdate(Player player) {  }

		/// <summary>
		/// Called when a player stops interacting with the object.
		/// </summary>
		public virtual void OnInteractionEnd(Player player) 
		{
			m_InteractionEndAudio.Play2D(); 
		}
	}
}