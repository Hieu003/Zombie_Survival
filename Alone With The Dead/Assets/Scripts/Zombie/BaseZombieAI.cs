using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using HQFPSWeapons;


public abstract class BaseZombieAI : MonoBehaviour
{
    // Các thuộc tính chung
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

    protected bool isAttacking;
    protected ZombieHealth zombieHealth;

    protected virtual void Start()
    {
        // Lấy các thành phần cần thiết
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        zombieHealth = GetComponent<ZombieHealth>();

        zombieHealth.OnZombieDeath += HandleZombieDeath;

        lastAttackTime = -attackCooldown;

        // Tùy biến tốc độ cho từng zombie
        if (navAgent != null)
            navAgent.speed = speed;
    }

    private void Update()
    {
        if (currentState == ZombieState.Dead)
            return;

        animator.SetFloat("MoveSpeed", navAgent.velocity.magnitude);
        HandleState();
    }

    // Xử lý trạng thái chung
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
            navAgent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
            currentState = ZombieState.Attack;
    }

    protected virtual void AttackBehavior()
    {
        animator.SetBool("IsAttacking", true);

        if (navAgent != null)
            navAgent.SetDestination(transform.position);

        if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            StartCoroutine(AttackWithDelay());

        if (Vector3.Distance(transform.position, player.position) > attackDistance)
            currentState = ZombieState.Chase;
    }

    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDelay);

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            PerformAttack();
        }

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    // Logic khi zombie chết
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

    // Phương thức trừu tượng để các lớp con triển khai
    protected abstract void PerformAttack();
}
