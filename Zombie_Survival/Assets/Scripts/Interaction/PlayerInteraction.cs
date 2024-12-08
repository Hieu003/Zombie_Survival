using UnityEngine;

namespace HQFPSWeapons
{
	/// <summary>
	/// Sends a ray from the center of the camera, in the game world.
	/// Gathers data about what is in front of the player, and stores it in a variable.
	/// </summary>
	public class PlayerInteraction : PlayerComponent
	{
		[SerializeField]
		private Camera m_WorldCamera = null;

		[SerializeField] 
		[Tooltip("The maximum distance at which you can interact with objects.")]
		private float m_InteractionDistance = 2f;

		[SerializeField]
		[Range(0f, 60f)]
		private float m_MaxInteractionAngle = 30f;

		[SerializeField]
		private LayerMask m_LayerMask = new LayerMask();

		private InteractiveObject m_InteractedObject;


		private void Awake()
		{
			Player.WantsToInteract.AddChangeListener(OnChanged_WantsToInteract);
		}

		private void OnChanged_WantsToInteract(bool wantsToInteract)
		{
			var raycastData = Player.RaycastData.Get();

			var wantedToInteractPreviously = Player.WantsToInteract.GetPreviousValue();
			var wantsToInteractNow = wantsToInteract;

			if(raycastData != null && raycastData.IsInteractive)
			{
				if(!wantedToInteractPreviously && wantsToInteractNow)
				{
					raycastData.InteractiveObject.OnInteractionStart(Player);
					m_InteractedObject = raycastData.InteractiveObject;
				}
			}

			if(m_InteractedObject != null && wantedToInteractPreviously && !wantsToInteractNow)
			{
				m_InteractedObject.OnInteractionEnd(Player);
				m_InteractedObject = null;
			}
		}

		private void Update()
		{
			var ray = m_WorldCamera.ViewportPointToRay(Vector2.one * 0.5f);
			var lastRaycastData = Player.RaycastData.Get();

			var collidersInRange = Physics.OverlapSphere(m_WorldCamera.transform.position, m_InteractionDistance, m_LayerMask, QueryTriggerInteraction.Collide);
			float smallestAngle = 1000f;
			InteractiveObject closestObject = null;
			int closestObjectIndex = -1;

			Vector3 cameraPosition = m_WorldCamera.transform.position;
			Vector3 cameraDirection = m_WorldCamera.transform.forward;

			for(int i = 0;i < collidersInRange.Length; i ++)
			{
				InteractiveObject interactiveObject = collidersInRange[i].GetComponent<InteractiveObject>();

				RaycastHit hitInfo;

				if(interactiveObject != null && Physics.Linecast(cameraPosition, interactiveObject.transform.position + (interactiveObject.transform.position - cameraPosition).normalized * 0.05f, out hitInfo, m_LayerMask))
				{
					if(hitInfo.collider == null || hitInfo.collider == collidersInRange[i])
					{
						float angle = Vector3.Angle(cameraDirection, interactiveObject.transform.position - cameraPosition);

						if(angle < smallestAngle)
						{
							smallestAngle = angle;
							closestObject = interactiveObject;
							closestObjectIndex = i;
						}
					}
				}
			}

			if(smallestAngle < m_MaxInteractionAngle && ((lastRaycastData != null && lastRaycastData.Collider != collidersInRange[closestObjectIndex]) || lastRaycastData == null))
			{
				var raycastData = new RaycastData(collidersInRange[closestObjectIndex], closestObject);
				Player.RaycastData.Set(raycastData);

				bool startedRaycastingOnObject =
					lastRaycastData != null && 
					raycastData.IsInteractive && 
					raycastData.InteractiveObject != lastRaycastData.InteractiveObject;

				if(startedRaycastingOnObject)
					raycastData.InteractiveObject.OnRaycastStart(Player);
				else if(raycastData.IsInteractive)
					raycastData.InteractiveObject.OnRaycastUpdate(Player);
				else if(lastRaycastData != null && lastRaycastData.InteractiveObject != null)
					lastRaycastData.InteractiveObject.OnRaycastEnd(Player);
			}
			else if(smallestAngle > m_MaxInteractionAngle)
			{
				Player.RaycastData.Set(null);

				// Let the object know the ray it's not on it anymore.
				if(lastRaycastData != null && lastRaycastData.IsInteractive)
				{
					if(lastRaycastData.IsInteractive)
						lastRaycastData.InteractiveObject.OnRaycastEnd(Player);
				}
			}

//			if(Physics.Raycast(ray, out hitInfo, m_InteractionDistance, m_LayerMask, QueryTriggerInteraction.Collide) && ((lastRaycastData != null && lastRaycastData.HitInfo.collider != hitInfo.collider) || lastRaycastData == null))
//			{
//				var raycastData = new RaycastData(hitInfo);
//				Player.RaycastData.Set(raycastData);
//
//				bool startedRaycastingOnObject =
//					lastRaycastData != null && 
//					raycastData.IsInteractive && 
//					raycastData.InteractiveObject != lastRaycastData.InteractiveObject;
//
//				if(startedRaycastingOnObject)
//					raycastData.InteractiveObject.OnRaycastStart(Player);
//				else if(raycastData.IsInteractive)
//					raycastData.InteractiveObject.OnRaycastUpdate(Player);
//				else if(lastRaycastData != null && lastRaycastData.InteractiveObject != null)
//					lastRaycastData.InteractiveObject.OnRaycastEnd(Player);
//			}
//			else if(hitInfo.collider == null)
//			{
//				Player.RaycastData.Set(null);
//
//				// Let the object know the ray it's not on it anymore.
//				if(lastRaycastData != null && lastRaycastData.IsInteractive)
//				{
//					if(lastRaycastData.IsInteractive)
//						lastRaycastData.InteractiveObject.OnRaycastEnd(Player);
//				}
//			}

//			bool objectInProximity = Physics.CheckSphere(transform.position + transform.TransformVector(m_ProximityDetection.Offset), m_ProximityDetection.Radius, m_ProximityDetection.Mask);
//
//			Player.ObjectInProximity.Set(objectInProximity);

			if(m_InteractedObject != null)
				m_InteractedObject.OnInteractionUpdate(Player);
		}

//		private void OnDrawGizmosSelected()
//		{
//			Gizmos.color = Color.red;
//			Gizmos.DrawWireSphere(transform.position + transform.TransformVector(m_ProximityDetection.Offset), m_ProximityDetection.Radius);
//		}


//		// -------------------- Internal --------------------
//		[Serializable]
//		public struct ProximityDetection
//		{
//			public Vector3 Offset;
//
//			[Range(0f, 2f)]
//			public float Radius;
//
//			public LayerMask Mask;
//		}
	}
}