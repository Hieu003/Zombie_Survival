using UnityEngine;

namespace HQFPSWeapons
{
	public class Easer 
	{
		public float InterpolatedValue { get; private set; }

		public Easings.Function Function { get { return m_Function; } set { m_Function = value; } }
		public float Duration { get { return m_Duration; } set { m_Duration = value; m_Speed = 1f / m_Duration; } }

		private float m_Time;
		private float m_Duration;
		private Easings.Function m_Function;
		private float m_Speed;


		public Easer(Easings.Function function, float duration)
		{
            m_Function = function;
			m_Speed = 1f / duration;
		}

		public void Reset()
		{
			InterpolatedValue = 0f;
			m_Time = 0f;
		}

		public float Update(float deltaTime)
		{
			m_Time = Mathf.Clamp01(m_Time + m_Speed * deltaTime);
			InterpolatedValue = Easings.Interpolate(m_Time, Function);

			return InterpolatedValue;
		}
	}
}
