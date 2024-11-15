using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(DelayedCameraForce))]
	public class DelayedCamForceDrawer : CopyPasteBase<DelayedCameraForce>
	{
		private const string menuName = "Delayed Cam Force";


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

