using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// 
	/// </summary>
	public class LivingEntityComponent : MonoBehaviour 
	{
		public LivingEntity Entity
		{
			get 
			{
				if(!m_Entity)
					m_Entity = GetComponent<LivingEntity>();
				if(!m_Entity)
					m_Entity = GetComponentInParent<LivingEntity>();
				
				return m_Entity;
			}
		}

		private LivingEntity m_Entity;

		public virtual void OnEntityStart() {  }
	}
}
