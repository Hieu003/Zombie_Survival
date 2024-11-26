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
       // animator.SetBool("IsAttacking", false);

        if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
            currentState = ZombieState.Chase;
    }

    protected virtual void ChaseBehavior()
    {
        animator.SetBool("IsWalking", true);
 

        if (navAgent != null)
        {
            navAgent.SetDestination(player.position);
            navAgent.stoppingDistance = attackDistance;
        }

        // Chuyển sang Attack nếu trong khoảng cách Attack
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            currentState = ZombieState.Attack;
            
        }
       
    }


    protected virtual void AttackBehavior()
    {

        animator.SetBool("IsAttacking", true);

        // Dừng zombie tại vị trí hiện tại để tấn công
        if (navAgent != null)
            navAgent.SetDestination(transform.position);

        // Chỉ gây sát thương nếu không đang tấn công
        if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            StartCoroutine(AttackWithDelay());

        // Quay lại Chase nếu người chơi thoát khỏi attackDistance
        if (Vector3.Distance(transform.position, player.position) > attackDistance)
        {
            navAgent.stoppingDistance = 0f; // Reset khoảng cách dừng
            currentState = ZombieState.Chase;

        }
    }

    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;

        // Chờ thời gian delay trước khi gây sát thương
        yield return new WaitForSeconds(attackDelay);

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            PerformAttack(); // Gây sát thương
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance); // Phạm vi tấn công
    }

    protected abstract void PerformAttack();
}



