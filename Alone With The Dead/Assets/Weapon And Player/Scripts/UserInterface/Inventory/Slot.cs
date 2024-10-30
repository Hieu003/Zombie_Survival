using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HQFPSWeapons.UserInterface
{
	[ExecuteInEditMode]
	public abstract class Slot : UserInterfaceBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
	{
		public Message<Slot> Refresh = new Message<Slot>();
		public Message<Slot, PointerEventData> PointerDown = new Message<Slot, PointerEventData>();
		public Message<Slot, PointerEventData> PointerUp = new Message<Slot, PointerEventData>();

		public Message<PointerEventData, Slot> BeginDrag = new Message<PointerEventData, Slot>();
		public Message<PointerEventData, Slot> Drag = new Message<PointerEventData, Slot>();
		public Message<PointerEventData, Slot> EndDrag = new Message<PointerEventData, Slot>();

		public Message<State> StateChanged = new Message<State>();

		public ContainerInterface<Slot> BaseParent { get; private set; }

		[BHeader("General", true)]

		[SerializeField]
		protected Graphic _Graphic;

		[SerializeField]
		private Transition m_Transition;

		[SerializeField]
		private SoundPlayer m_PointerDownAudio = null;

		protected State m_State = State.Normal;

		private CanvasRenderer m_Renderer;
		protected bool m_Pressed;
		protected bool m_Selected;
		protected bool m_PointerHovering;


		public virtual void Select()
		{
			m_Selected = true;

            RefreshState(m_State);
		}

		public virtual void Deselect()
		{
            m_Selected = false;
		
			RefreshState(m_PointerHovering ? State.Highlighted : State.Normal);
		}

		public virtual void OnPointerEnter(PointerEventData data)
		{
            m_PointerHovering = true;

			if(!m_Pressed)
				RefreshState(State.Highlighted);
		}

		public virtual void OnPointerDown(PointerEventData data)
		{
            if(data.button == PointerEventData.InputButton.Left)
			{
                m_Pressed = true;
                RefreshState(State.Pressed);
			}

            m_PointerDownAudio.Play2D();

			PointerDown.Send(this, data);
		}

		public virtual void OnPointerUp(PointerEventData data)
		{
            m_Pressed = false;

			Slot slotUnderPointer = data.pointerCurrentRaycast.gameObject == null ? null : data.pointerCurrentRaycast.gameObject.GetComponent<Slot>();

            if(slotUnderPointer != null)
            {
                if(slotUnderPointer != this)
                    RefreshState(State.Normal);
                else
                    RefreshState(State.Highlighted);
            }
            else
                RefreshState(State.Normal);

            PointerUp.Send(this, data);
		}

		public virtual void OnPointerExit(PointerEventData data)
		{
            m_PointerHovering = false;
	
			if(!m_Pressed)
				RefreshState(State.Normal);
			else
				RefreshState(State.Pressed);
		}

		public void OnBeginDrag(PointerEventData data)
		{
            BeginDrag.Send(data, this);
		}

		public void OnDrag(PointerEventData data)
		{
            Drag.Send(data, this);
		}

		public void OnEndDrag(PointerEventData data)
		{
            EndDrag.Send(data, this);
		}

		protected virtual void Awake()
		{
			if(!Application.isPlaying)
				return;

			m_Renderer = GetComponent<CanvasRenderer>();
			BaseParent = GetComponentInParent<ContainerInterface<Slot>>();
		}

		protected virtual void OnEnable()
		{
			if(_Graphic == null)
				_Graphic = GetComponent<Graphic>();

			if(m_Transition == null)
				m_Transition = new Transition();

			OnValidate();
		}

		protected virtual void OnDisable()
		{	
			var r = m_Renderer == null ? GetComponent<CanvasRenderer>() : m_Renderer;

			if(r != null)
				r.SetColor(Color.white);
		}

		protected virtual void OnDestroy()
		{
			var r = m_Renderer == null ? GetComponent<CanvasRenderer>() : m_Renderer;

			if(r != null)
				r.SetColor(Color.white);
		}

		protected virtual void OnValidate()
		{
			var r = m_Renderer == null ? GetComponent<CanvasRenderer>() : m_Renderer;

			if(r != null)
				r.SetColor(m_Transition.NormalColor);
		}

		private void RefreshState(State state)
		{
			m_State = state;
            Color color = m_Transition.NormalColor;

			if(state == State.Highlighted)
				color = m_Transition.HighlightedColor;
			else if(state == State.Pressed)
				color = m_Transition.PressedColor;

			if(m_Selected)
				color = m_Transition.HighlightedColor;

			_Graphic.CrossFadeColor(color, m_Transition.FadeDuration, true, true);

			StateChanged.Send(m_State);
		}

        #region Internal
        public enum State
		{
			Normal,
			Highlighted,
			Pressed,
		}

		[Serializable]
		public class Transition
		{
			public Color NormalColor = Color.grey;

			public Color HighlightedColor = Color.grey;

			public Color PressedColor = Color.grey;

			[Clamp(0.01f, 1f)]
			public float FadeDuration = 0.1f;
		}
        #endregion
    }
}
