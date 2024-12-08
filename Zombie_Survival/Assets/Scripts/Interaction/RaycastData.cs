using UnityEngine;

namespace HQFPSWeapons
{
	public class RaycastData
	{
		public Collider Collider { get; private set; }

		/// <summary> </summary>
		public InteractiveObject InteractiveObject { get; private set; }

		/// <summary> </summary>
		public bool IsInteractive { get; private set; }


		public RaycastData(Collider collider, InteractiveObject interactiveObject)
		{
			Collider = collider;
			InteractiveObject = interactiveObject;
			IsInteractive = (InteractiveObject != null) && InteractiveObject.InteractionEnabled;
		}
	}
}