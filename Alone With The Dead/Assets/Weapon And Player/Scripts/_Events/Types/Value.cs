using System;
using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// This allows to have a callback when the value changes (An example would be updating the GUI when the player health changes).
	/// </summary>
	[Serializable]
	public class Value<T>
	{
		public delegate T Filter(T previousValue, T newValue);

		public T Val { get { return m_CurrentValue; } }
		public T PrevVal { get { return m_PreviousValue; } }

		[NonSerialized]
		private Action<T> m_Set;

		[NonSerialized]
		private Filter m_Filter;

		[SerializeField]
		private T m_CurrentValue;

		[SerializeField]
		private T m_PreviousValue;


		public Value()
		{
			m_CurrentValue = default(T);
			m_PreviousValue = default(T);
		}

		public Value(T initialValue)
		{
			m_CurrentValue = initialValue;
			m_PreviousValue = m_CurrentValue;
		}

		public void AddChangeListener(Action<T> callback)
		{
			m_Set += callback;
		}

		public void RemoveChangeListener(Action<T> callback)
		{
			m_Set -= callback;
		}

		/// <summary>A "filter" method will be called before the regular callbacks, useful for clamping values (like the player health, etc).</summary>
		public void SetFilter(Filter filter)
		{
			m_Filter = filter;
		}

		/// <summary>Returns the current value.</summary>
		public T Get()
		{
			return m_CurrentValue;
		}

		/// <summary>Returns the previous value.</summary>
		public T GetPreviousValue()
		{
			return m_PreviousValue;
		}

		public void Set(T value)
		{
			m_PreviousValue = m_CurrentValue;
			m_CurrentValue = value;

			if(m_Filter != null)
				m_CurrentValue = m_Filter(m_PreviousValue, m_CurrentValue);

			if(m_Set != null && ((m_PreviousValue == null && m_CurrentValue != null) || (m_PreviousValue != null && !m_PreviousValue.Equals(m_CurrentValue))))
				m_Set(m_CurrentValue);
		}

		public void SetAndForceUpdate(T value)
		{
			m_PreviousValue = m_CurrentValue;
			m_CurrentValue = value;

			if(m_Filter != null)
				m_CurrentValue = m_Filter(m_PreviousValue, m_CurrentValue);

			if(m_Set != null)
				m_Set(m_CurrentValue);
		}

		public void SetAndDontUpdate(T value)
		{
			m_PreviousValue = m_CurrentValue;
			m_CurrentValue = value;

			if(m_Filter != null)
				m_CurrentValue = m_Filter(m_PreviousValue, m_CurrentValue);
		}

		public bool Is(T value)
		{
			return m_CurrentValue != null && m_CurrentValue.Equals(value);
		}
	}
}