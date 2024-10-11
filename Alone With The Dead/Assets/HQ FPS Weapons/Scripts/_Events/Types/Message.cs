using System;
using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// 
	/// </summary>
	public class Message
	{
		public float LastCallTime { get { return m_CallTime; } private set { } }

		private Action m_Callbacks;

		private float m_CallTime;


		/// <summary>
		/// 
		/// </summary>
		public void AddListener(Action callback)
		{
			m_Callbacks += callback;
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveListener(Action callback)
		{
			m_Callbacks -= callback;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Send()
		{
			if (m_Callbacks != null)
			{
				m_CallTime = Time.time;

				m_Callbacks();
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class Message<T>
	{
		private Action<T> m_Callbacks;


		/// <summary>
		/// 
		/// </summary>
		public void AddListener(Action<T> callback)
		{
			m_Callbacks += callback;
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveListener(Action<T> callback)
		{
			m_Callbacks -= callback;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Send(T arg)
		{
			if(m_Callbacks != null)
				m_Callbacks(arg);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class Message<T, V>
	{
		private Action<T, V> m_Callbacks;


		/// <summary>
		/// 
		/// </summary>
		public void AddListener(Action<T, V> callback)
		{
			m_Callbacks += callback;
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveListener(Action<T, V> callback)
		{
			m_Callbacks -= callback;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Send(T arg1, V arg2)
		{
			if(m_Callbacks != null)
				m_Callbacks(arg1, arg2);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class Message<T, V, K>
	{
		private Action<T, V, K> m_Callbacks;


		public void AddListener(Action<T, V, K> callback)
		{
			m_Callbacks += callback;
		}

		public void RemoveListener(Action<T, V, K> callback)
		{
			m_Callbacks -= callback;
		}

		public void Send(T arg1, V arg2, K arg3)
		{
			if(m_Callbacks != null)
				m_Callbacks(arg1, arg2, arg3);
		}
	}
}

