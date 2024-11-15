using System;
using UnityEngine;

namespace HQFPSWeapons
{
	[AttributeUsage(AttributeTargets.Field)]
    public class ShowIf : PropertyAttribute
    {
        public readonly string m_PropertyName;
        public readonly float m_Indentation;

        public readonly bool m_RequiredBool = false;
		public readonly int m_RequiredInt = -1;
		public readonly float m_RequiredFloat = -1f;
        public readonly string m_RequiredString = "s";
        public readonly Vector3 m_RequiredVector3 = new Vector3(1, 1, 1);


		public ShowIf(string propertyName, bool requiredValue, float indentation = 16)
        {
            m_PropertyName = propertyName;
            m_RequiredBool = requiredValue;

            m_Indentation = indentation;
        }

		public ShowIf(string propertyName, int requiredValue, float indentation = 16)
		{
			m_PropertyName = propertyName;
			m_RequiredInt = requiredValue;

			m_Indentation = indentation;
		}

		public ShowIf(string propertyName, float requiredValue, float indentation = 16)
		{
			m_PropertyName = propertyName;
			m_RequiredFloat = requiredValue;

			m_Indentation = indentation;
		}
			
        public ShowIf(string propertyName, string requiredValue, float indentation = 16)
        {
            m_PropertyName = propertyName;
            m_RequiredString = requiredValue;

            m_Indentation = indentation;
        }
			
		public ShowIf(string propertyName, Vector3 requiredValue, float indentation = 16)
        {
            m_PropertyName = propertyName;
            m_RequiredVector3 = requiredValue;

            m_Indentation = indentation;
        }
    }
}