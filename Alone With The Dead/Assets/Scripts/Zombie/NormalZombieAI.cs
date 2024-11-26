using System.Collections;
using UnityEngine;
using HQFPSWeapons;
    

public class NormalZombieAI : BaseZombieAI
{
    protected override void Start()
    {
        // Thiết lập các chỉ số giống với ZombieAI
        speed = 1f;
        chaseDistance = 8f;
        attackDistance = .9f;
        attackCooldown = .8f;
        attackDelay = .8f;

        base.Start(); // Gọi hàm Start của BaseZombieAI
    }

    protected override void PerformAttack()
    {
        // Gây sát thương chỉ khi người chơi còn trong phạm vi tấn công
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
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
