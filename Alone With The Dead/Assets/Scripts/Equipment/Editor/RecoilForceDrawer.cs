using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(RecoilForce))]
	public class RecoilForceDrawer : CopyPasteBase<RecoilForce>
	{
		private const string menuName = "Recoil Force";


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
