using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HQFPSWeapons 
{
	public static class Extensions
	{
		public static string DoUnityLikeNameFormat(this string str)
		{
			if(string.IsNullOrEmpty(str))
				return string.Empty;

			if(str.Length > 2 && str[0] == 'm' && str[1] == '_')
				str = str.Remove(0, 2);

			if(str.Length > 1 && str[0] == '_')
				str = str.Remove(0);

			StringBuilder newText = new StringBuilder(str.Length * 2);
			newText.Append(str[0]);

			for(int i = 1; i < str.Length; i++)
			{
				bool lastIsUpper = char.IsUpper(str[i - 1]);
				bool lastIsSpace = str[i - 1] ==  ' ';
				bool lastIsDigit = char.IsDigit(str[i - 1]);

				if(char.IsUpper(str[i]) && !lastIsUpper && !lastIsSpace)
					newText.Append(' ');

				if(char.IsDigit(str[i]) && !lastIsDigit && !lastIsUpper && !lastIsSpace)
					newText.Append(' ');

				newText.Append(str[i]);
			}

			return newText.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		public static Transform FindDeepChild(this Transform parent, string childName) 
		{
			var result = parent.Find(childName);

			if(result) 
				return result;

			for(int i = 0;i < parent.childCount;i ++)
			{
				result = parent.GetChild(i).FindDeepChild(childName);
				if(result)
					return result;
			}

			return null;
		}

		/// <summary>
		/// Checks if the index is inside the list's bounds.
		/// </summary>
		public static bool IndexIsValid<T>(this List<T> list, int index)
		{
			return index >= 0 && index < list.Count;
		}

        public static List<T> CopyOther<T>(this List<T> list, List<T> toCopy)
        {
            if (toCopy == null || toCopy.Count == 0)
                return null;

            list = new List<T>();

            for (int i = 0; i < toCopy.Count; i++)
                list.Add(toCopy[i]);

            return list;
        }

        public static bool IsInRangeLimitsExcluded(this float f, float l1, float l2)
        {
            return f > l1 && f < l2;
        }

		public static bool IsInRangeLimitsIncluded(this float f, float l1, float l2)
		{
			return f >= l1 && f <= l2;
		}
    }
}