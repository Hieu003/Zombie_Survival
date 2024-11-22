using System.Collections;
using UnityEngine;
using HQFPSWeapons;

public class FastZombieAI : BaseZombieAI
{
    protected override void Start()
    {
        // Cấu hình các chỉ số riêng cho FastZombie
        speed = 3f; // Tốc độ nhanh hơn
        chaseDistance = 12f; // Khoảng cách phát hiện xa hơn
        attackDistance = 1f; // Khoảng cách tấn công gần hơn
        attackCooldown = .5f; // Thời gian hồi chiêu nhanh hơn
        attackDelay = .5f; // Đánh nhanh hơn

        // Gọi hàm Start của BaseZombieAI
        base.Start();

        // Thiết lập máu thấp hơn NormalZombie
        if (zombieHealth != null)
            zombieHealth.SetInitialHealth(80f); // Máu ít hơn
    }

    protected override void PerformAttack()
    {

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            // Gây sát thương lên người chơi
            var playerVitals = player.GetComponent<PlayerVitals>();
            if (playerVitals != null)
            {
                HealthEventData damageData = new HealthEventData(-15f); // Gây sát thương nhỏ hơn
                playerVitals.Entity.ChangeHealth.Try(damageData);
            }
        }
    }

    protected override void HandleZombieDeath()
    {
        base.HandleZombieDeath(); // Gọi logic chết từ BaseZombieAI

        Debug.Log("FastZombie đã chết!");
        // Thêm hiệu ứng nếu cần, ví dụ: rơi vật phẩm.
    }
}
