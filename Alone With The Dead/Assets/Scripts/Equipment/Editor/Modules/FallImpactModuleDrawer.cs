using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(EquipmentPhysics.FallImpactModule))]
	public class FallImpactModuleDrawer : CopyPasteBase<EquipmentPhysics.FallImpactModule>
	{
		private const string menuName = "Fall Impact";


		[MenuItem("CONTEXT/" + menuName + "/Copy " + menuName)]
		private static void Copy()
		{
			DoCopy();
		}

		[MenuItem("CONTEXT/" + menuName + "/Paste " + menuName)]
		private static void Paste()
		{
			DoPaste();
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			OnGUI(position, property, label, menuName);
		}
	}
}
