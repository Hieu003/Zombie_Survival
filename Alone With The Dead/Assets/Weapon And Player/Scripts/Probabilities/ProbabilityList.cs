using System;
using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	[Serializable]
	public class ProbabilityList<T> where T : UnityEngine.Object
	{
		public int Count { get { return m_Items.Count; } }

		[SerializeField]
		private List<T> m_Items = null;

		[SerializeField]
		private List<int> m_Probabilities = null;


		public T GetRandomItem()
		{
			if(m_Items.Count > 0)
				return m_Items[ProbabilityUtils.RandomChoiceFollowingDistribution(m_Probabilities.ConvertAll((int p)=> { return (float)p; }))];
			else
				return default(T);
		}

		public bool GetRandomItems(int iterations, out T[] items)
		{
			iterations = Mathf.Clamp(iterations, 0, m_Items.Count / 2);

			if(iterations > 0)
			{
				items = new T[iterations];

				var clonedElements = new List<ProbabilityElement>();

				for(int e = 0;e < m_Items.Count;e ++)
					clonedElements.Add(new ProbabilityElement() { Item = m_Items[e], Probability = ((float)m_Probabilities[e]) / 100f });

				for(int i = 0;i < iterations;i ++)
				{
					int chosenElement = GetRandomItem(clonedElements);
					items[i] = m_Items[chosenElement];

					clonedElements.RemoveAt(chosenElement);
				}

				return true;
			}
			else
			{
				items = default(T[]);
				return false;
			}
		}

		private int GetRandomItem(List<ProbabilityElement> elements)
		{
			var probabilities = new List<float>();

			for(int i = 0;i < elements.Count;i ++)
				probabilities.Add(elements[i].Probability);

			return ProbabilityUtils.RandomChoiceFollowingDistribution(probabilities);
		}


        #region Internal
        [Serializable]
		public struct ProbabilityElement
		{
			public T Item;
			public float Probability;
		}
        #endregion
    }

    [Serializable]
	public class ItemPickupRandomList : ProbabilityList<ItemPickup> {  }
}