using System;
using UnityEngine;

namespace HQFPSWeapons
{
	public class ItemProperty
	{
		[Serializable]
		public struct Float
		{
			public float Current { get { return m_Current; } set { m_Current = value; } }

			/// <summary>The default value, that was set up initially, when the item was defined.</summary>
			public float Default { get { return m_Default; } }

			/// <summary>This is equal to Current / Default.</summary>
			public float Ratio { get { return m_Current / m_Default; } }

			#pragma warning disable 0649

			[SerializeField]
			private float m_Current;

			[SerializeField]
			private float m_Default;

			#pragma warning restore 0649

			public override string ToString()
			{
				return m_Current.ToString();
			}
		}
			
		[Serializable]
		public struct Definition
		{
			public string Name { get { return m_Name; } }

			#pragma warning disable 0649

			[SerializeField]
			private string m_Name;

			#pragma warning restore 0649
		}

		[Serializable]
		public class Value
		{
			[NonSerialized]
			public Message<Value> Changed = new Message<Value>();

			public string Name { get { return m_Name; } }

			public Float Val { get { return m_Float; } }

			[SerializeField]
			private string m_Name = string.Empty;

			[SerializeField]
			private Float m_Float = new Float();


			public Value GetClone()
			{
				return (Value)MemberwiseClone();
			}

			public void SetValue(object value)
			{
				m_Float = (Float)value;

				Changed.Send(this);
			}

			public override string ToString()
			{
				m_Float.ToString();

				return m_Name;
			}

			public void AdjustValue(float adjustment, float clampMin = -Mathf.Infinity, float clampMax = Mathf.Infinity)
			{
				var floatingPointNr = m_Float;
				floatingPointNr.Current = Mathf.Clamp(floatingPointNr.Current + adjustment, clampMin, clampMax);
				SetValue(floatingPointNr);
			}
		}
	}
}
