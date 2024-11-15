using System.Collections.Generic;
using UnityEngine;

namespace HQFPSWeapons
{
	public static class ItemSelection
	{
		public static T Select<T>(this T[] array, ref int last, Method selectionMethod = Method.Random)
		{
			if(array == null || array.Length == 0)
				return default(T);

			int next = 0;

			if(selectionMethod == Method.Random)
				next = Random.Range(0, array.Length);
			else if(selectionMethod == Method.RandomExcludeLast && array.Length > 1)
			{
				last = Mathf.Clamp(last, 0, array.Length - 1);

				T first = array[0];
				array[0] = array[last];
				array[last] = first;

				next = Random.Range(1, array.Length);
			}
			else if(selectionMethod == Method.Sequence)
				next = (int)Mathf.Repeat(last + 1, array.Length);

			last = next;

			return array[next];
		}

		public static T Select<T>(this List<T> list, ref int last, Method selectionMethod = Method.Random)
		{
			if(list == null || list.Count == 0)
				return default(T);

			int next = 0;

			if(selectionMethod == Method.Random)
				next = Random.Range(0, list.Count);
			else if(selectionMethod == Method.RandomExcludeLast && list.Count > 1)
			{
				last = Mathf.Clamp(last, 0, list.Count - 1);

				T first = list[0];
				list[0] = list[last];
				list[last] = first;

				next = Random.Range(1, list.Count);
			}
			else if(selectionMethod == Method.Sequence)
				next = (int)Mathf.Repeat(last + 1, list.Count);

			last = next;

			return list[next];
		}
			

		// ------------------------ Internal Declarations ------------------------
		public enum Method
		{
			/// <summary>The item will be selected randomly.</summary>
			Random,

			/// <summary>The item will be selected randomly, but will exclude the last selected.</summary>
			RandomExcludeLast,

			/// <summary>The items will be selected in sequence.</summary>
			Sequence
		}
	}
}
