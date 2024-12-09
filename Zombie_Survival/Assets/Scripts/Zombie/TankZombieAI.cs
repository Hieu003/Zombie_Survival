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

        // Bỏ qua trạng thái Walk, bắt đầu bằng Idle
        currentState = ZombieState.Idle;
        stateStartTime = Time.time;
    }
    protected override void HandleState()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                IdleBehavior();
                break;
            case ZombieState.Chase:
                ChaseBehavior();
                break;
            case ZombieState.Attack:
                AttackBehavior();
                break;
        }
    }

    // Loại bỏ WalkBehavior()

    protected override void IdleBehavior()
    {
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsAttacking", false);
        

        // Chuyển sang Chase nếu thấy người chơi
        if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
        {
            currentState = ZombieState.Chase;
            stateStartTime = Time.time;
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