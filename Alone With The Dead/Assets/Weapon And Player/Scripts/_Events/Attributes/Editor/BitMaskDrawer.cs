using System;
using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(BitMask))]
	public class EnumBitMaskPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			var attr = attribute as BitMask;

			prop.intValue = DrawBitMaskField(position, prop.intValue, attr.EnumType, label);
		}

		private int DrawBitMaskField(Rect position, int mask, Type type, GUIContent label)
		{
			var itemNames = Enum.GetNames(type);
			var itemValues = Enum.GetValues(type) as int[];

			int val = mask;
			int maskVal = 0;

			for(int i = 0; i < itemValues.Length; i++)
			{
				if (itemValues[i] != 0)
				{
					if ((val & itemValues[i]) == itemValues[i])
						maskVal |= 1 << i;
				}
				else if (val == 0)
					maskVal |= 1 << i;
			}

			int newMaskVal = EditorGUI.MaskField(position, label, maskVal, itemNames);
			int changes = maskVal ^ newMaskVal;

			for(int i = 0; i < itemValues.Length; i++)
			{
				if((changes & (1 << i)) != 0)            // has this list item changed?
				{
					if((newMaskVal & (1 << i)) != 0)     // has it been set?
					{
						if(itemValues[i] == 0)           // special case: if "0" is set, just set the val to 0
						{
							val = 0;
							break;
						}
						else
							val |= itemValues[i];
					}
					else                                  // it has been reset
						val &= ~itemValues[i];
				}
			}

			return val;
		}
	}
}