using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(EquipmentMotionState.NoiseModule))]
	public class NoiseModuleDrawer : CopyPasteBase<EquipmentMotionState.NoiseModule>
	{
		private const string menuName = "Noise";


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
