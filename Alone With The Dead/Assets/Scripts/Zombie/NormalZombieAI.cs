using System.Collections;
using UnityEngine;
using HQFPSWeapons;
    

public class NormalZombieAI : BaseZombieAI
{
    protected override void Start()
    {
        // Thiết lập các chỉ số giống với ZombieAI
        speed = 1f;
        chaseDistance = 10f;
        attackDistance = 1f;
        attackCooldown = 1f;
        attackDelay = 1f;

        base.Start(); // Gọi hàm Start của BaseZombieAI
    }

    protected override void PerformAttack()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            // Gây sát thương lên người chơi
            var playerVitals = player.GetComponent<PlayerVitals>();
            if (playerVitals != null)
            {
                HealthEventData damageData = new HealthEventData(-20f); // Gây sát thương -20
                playerVitals.Entity.ChangeHealth.Try(damageData);
            }
        }
    }

    protected override void HandleZombieDeath()
    {
        base.HandleZombieDeath(); // Gọi logic chết từ BaseZombieAI

        // Nếu cần, có thể thêm logic khác (ví dụ rơi vật phẩm).
    }
}
