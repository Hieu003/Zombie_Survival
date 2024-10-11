using System;
using UnityEngine;

namespace HQFPSWeapons
{
	public class BitMask : PropertyAttribute
	{
		public Type EnumType;


		public BitMask(Type enumType)
		{
			EnumType = enumType;
		}
	}
}