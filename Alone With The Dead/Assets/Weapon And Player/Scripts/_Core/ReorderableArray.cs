using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HQFPSWeapons {

	[Serializable]
	public abstract class ReorderableArray<T> : ICloneable, IList<T>, ICollection<T>, IEnumerable<T> {

		public List<T> List { get { return m_List; } set { m_List = value; } }

		[SerializeField]
		private List<T> m_List = new List<T>();


		public ReorderableArray(): this(0) {  }

		public ReorderableArray(int length) 
		{
			m_List = new List<T>(length);
		}

		public T this[int index] 
		{
			get { return m_List[index]; }
			set { m_List[index] = value; }
		}
		
		public int Length
		{
			get { return m_List.Count; }
		}

		public bool IsReadOnly 
		{
			get { return false; }
		}

		public int Count
		{
			get { return m_List.Count; }
		}

		public object Clone() {

			return new List<T>(m_List);
		}

		public bool Contains(T value) {

			return m_List.Contains(value);
		}

		public int IndexOf(T value) {

			return m_List.IndexOf(value);
		}

		public void Insert(int index, T item) {

			m_List.Insert(index, item);
		}

		public void RemoveAt(int index) {

			m_List.RemoveAt(index);
		}

		public void Add(T item) {

			m_List.Add(item);
		}

		public void Clear() {

			m_List.Clear();
		}

		public void CopyTo(T[] array, int arrayIndex) {

			this.m_List.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item) {

			return m_List.Remove(item);
		}

		public T[] ToArray() {

			return m_List.ToArray();
		}

		public IEnumerator<T> GetEnumerator() {

			return m_List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {

			return m_List.GetEnumerator();
		}
	}
}
