using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using HQFPSWeapons;

public abstract class BaseZombieAI : MonoBehaviour
{
    public enum ZombieState { Idle, Chase, Attack, Dead }
    public ZombieState currentState = ZombieState.Idle;

    public NavMeshAgent navAgent;
    protected Animator animator;
    public Transform player;

    // Các thông số chung
    protected float speed;
    protected float chaseDistance;
    protected float attackDistance;
    protected float attackCooldown;
    protected float attackDelay;
    protected float lastAttackTime;
    protected float bufferZone = 1f; // Vùng đệm giữa Chase và Attack

    protected bool isAttacking;
    protected ZombieHealth zombieHealth;

    protected virtual void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        zombieHealth = GetComponent<ZombieHealth>();

        zombieHealth.OnZombieDeath += HandleZombieDeath;

        lastAttackTime = -attackCooldown;

        if (navAgent != null)
            navAgent.speed = speed;
    }

    private void Update()
    {
        if (currentState == ZombieState.Dead)
            return;

  
        HandleState();
    }

    protected virtual void HandleState()
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

    protected virtual void IdleBehavior()
    {
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsAttacking", false);

        if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
            currentState = ZombieState.Chase;
    }

    protected virtual void ChaseBehavior()
    {
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsAttacking", false);

        if (navAgent != null)
        {
            navAgent.SetDestination(player.position);
            navAgent.stoppingDistance = attackDistance; // Đồng bộ khoảng cách dừng
        }

        // Chuyển sang Attack nếu trong khoảng cách Attack
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
            currentState = ZombieState.Attack;
    }


    protected virtual void AttackBehavior()
    {
        animator.SetBool("IsAttacking", true);

        // Dừng tại vị trí hiện tại
        if (navAgent != null)
            navAgent.SetDestination(transform.position);

        // Chỉ gây sát thương nếu không đang tấn công
        if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            StartCoroutine(AttackWithDelay());

        // Quay lại Chase nếu người chơi vượt ngoài vùng đệm
        if (Vector3.Distance(transform.position, player.position) > attackDistance + bufferZone)
        {
            currentState = ZombieState.Chase;
            navAgent.stoppingDistance = 0f; // Reset khoảng cách dừng
        }
    }


    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDelay);

        // Kiểm tra va chạm trong khoảng attackDistance
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackDistance);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform == player)
            {
                PerformAttack(); // Gây sát thương nếu phát hiện người chơi
                break;
            }
        }

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    protected virtual void HandleZombieDeath()
    {
        currentState = ZombieState.Dead;

        if (navAgent != null)
            navAgent.isStopped = true;

        animator.SetBool("IsDead", true);

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;

        StartCoroutine(RemoveZombieAfterDelay(30f));
    }

    private IEnumerator RemoveZombieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    protected abstract void PerformAttack();
}
