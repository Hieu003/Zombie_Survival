using System.Collections;
using UnityEngine;
using HQFPSWeapons;


public class TankZombieAI : BaseZombieAI
{
    // Các thông số riêng của TankZombie
    private const float TankSpeed = .5f; // Tốc độ chậm hơn
    private const float TankChaseDistance = 12f; // Khoảng cách truy đuổi
    private const float TankAttackDistance = 1f; 
    private const float TankAttackCooldown = 3f; 
    private const float TankAttackDelay = 2f; // Thời gian chờ tấn công lâu hơn
    private const float TankDamage = 50f; // Sát thương cao hơn

    protected override void Start()
    {
        // Gán thông số riêng
        speed = TankSpeed;
        chaseDistance = TankChaseDistance;
        attackDistance = TankAttackDistance;
        attackCooldown = TankAttackCooldown;
        attackDelay = TankAttackDelay;

        base.Start();

        // Thiết lập máu cho TankZombie
        if (zombieHealth != null)
        {
            zombieHealth.SetInitialHealth(200f); // Máu của TankZombie là 200
        }
    }

    // Cụ thể hóa hành vi tấn công cho TankZombie
    protected override void PerformAttack()
    {
        if (player == null)
            return;

        // Gây sát thương lên nhân vật
        var playerVitals = player.GetComponent<PlayerVitals>();
        if (playerVitals != null)
        {
            HealthEventData damageData = new HealthEventData(-TankDamage); // Sát thương -50
            playerVitals.Entity.ChangeHealth.Try(damageData);
        }

        Debug.Log("TankZombie tấn công người chơi!");
    }
}
