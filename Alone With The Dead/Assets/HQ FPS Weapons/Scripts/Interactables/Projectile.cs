using UnityEngine;

namespace HQFPSWeapons
{
	public abstract class Projectile : MonoBehaviour
	{
		public abstract void Launch(LivingEntity launcher);
	}
}