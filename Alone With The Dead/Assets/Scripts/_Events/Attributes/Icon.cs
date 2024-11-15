using System;
using UnityEngine;

namespace HQFPSWeapons
{
	public class Icon : PropertyAttribute
	{
		public readonly int Size;


		public Icon(int size = 64)
		{
			Size = size;
		}
	}
}