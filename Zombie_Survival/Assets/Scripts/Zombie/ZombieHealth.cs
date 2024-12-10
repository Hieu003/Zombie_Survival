using UnityEngine;
using System;

namespace HQFPSWeapons
{
    public class ZombieHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField]
        private float initialHealth = 100f;

        [SerializeField]
        private DamageResistance damageResistance = null;

        [Header("Death Settings")]
        [SerializeField]
        private GameObject ragdollPrefab = null;

        [SerializeField]
        private Vector3 ragdollForceOffset = Vector3.zero;

        [Header("Blood Effect Settings")] // Thêm header cho máu
        [SerializeField]
        private ParticleSystem bloodEffectPrefab; // Prefab Particle System máu

        private float currentHealth;

        public event Action OnZombieDeath; // Sự kiện khi zombie chết

        private void Start()
        {
            currentHealth = initialHealth;
        }

        public void TakeDamage(HealthEventData damageData)
        {
            float damage = -Mathf.Abs(damageData.Delta);

            // Áp dụng kháng sát thương (DamageResistance)
            damage *= (1f - damageResistance.GetDamageResistance(damageData));

            // Trừ máu
            currentHealth = Mathf.Clamp(currentHealth + damage, 0f, initialHealth);

            // Tạo hiệu ứng máu tại vị trí trúng đạn
            SpawnBloodEffect(damageData.HitPoint, damageData.HitNormal);

            if (currentHealth <= 0)
                Die(damageData);
        }

        private void Die(HealthEventData damageData)
        {
            // Gọi sự kiện OnZombieDeath
            OnZombieDeath?.Invoke();

            // Nếu cần, thực hiện các logic khác như thêm hiệu ứng ragdoll.
            if (ragdollPrefab != null)
            {
                GameObject ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);
                Rigidbody[] ragdollParts = ragdoll.GetComponentsInChildren<Rigidbody>();

                foreach (var part in ragdollParts)
                {
                    part.AddForce(damageData.HitDirection * damageData.HitImpulse + ragdollForceOffset, ForceMode.Impulse);
                }
            }
        }

        private void SpawnBloodEffect(Vector3 position, Vector3 normal)
        {
            if (bloodEffectPrefab != null)
            {
                ParticleSystem bloodEffect = Instantiate(bloodEffectPrefab, position, Quaternion.LookRotation(normal));
                Destroy(bloodEffect.gameObject, bloodEffect.main.duration); // Hủy hiệu ứng sau khi chạy xong
            }
        }

        public LivingEntity GetEntity()
        {
            return null;
        }

        public void SetInitialHealth(float health)
        {
            initialHealth = Mathf.Max(1f, health); // Đảm bảo giá trị >= 1
            currentHealth = initialHealth; // Reset lại máu hiện tại
        }
        public void ResetHealth()
        {
            currentHealth = initialHealth;
        }
    }

    
}
