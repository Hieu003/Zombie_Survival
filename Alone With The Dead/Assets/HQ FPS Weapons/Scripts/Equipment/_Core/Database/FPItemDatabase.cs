using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// Represents an asset that stores all the user-defined data for the first person equipment
	/// </summary>
	[CreateAssetMenu(menuName = "HQ FPS Weapons Pack/First Person Item Database")]
	public class FPItemDatabase : ScriptableObject
	{
		public static FPItemDatabase Default
		{
			get
			{
				if (m_Default == null)
				{
					var allDatabases = Resources.LoadAll<FPItemDatabase>("");
					if (allDatabases != null && allDatabases.Length > 0)
						m_Default = allDatabases[0];
				}

				return m_Default; 
			}
		}

		[SerializeField]
		private FPItem m_FPUnarmed = null;

		[SerializeField]
		private FPItem[] m_FPItems = null;

		private static FPItemDatabase m_Default;
		private Dictionary<string, FPItem> m_Items = new Dictionary<string, FPItem>();


		public FPItem GetFPItemData(string name) 
		{
			if (string.IsNullOrEmpty(name))
				return m_FPUnarmed;
			else if (m_Items.TryGetValue(name, out FPItem fpItemData))
				return fpItemData;
			else
				return m_FPUnarmed;
		}

		private void OnEnable()
		{
			GenerateDictionaries();
		}

		private void OnValidate()
		{
			GenerateDictionaries();
		}

		private void GenerateDictionaries()
		{
			m_Items = new Dictionary<string, FPItem>();

			for (int i = 0; i < m_FPItems.Length; i++)
			{
				var item = m_FPItems[i];
				if (!m_Items.ContainsKey(item.Name))
				{
					m_Items.Add(item.Name, item);
				}
			}
		}
	}
}
