namespace HQFPSWeapons
{
	public interface IDamageable
	{
		void TakeDamage(HealthEventData damageData);

		LivingEntity GetEntity();
	}
}