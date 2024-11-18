using HQFPSWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{



    public NavMeshAgent navAgent;

    public enum ZombieState { Idle, Chase, Attack, Dead};

    public Animator animator;
 
    public ZombieState currentState = ZombieState.Idle;

    public Transform player;

    public float zombieSpeed = 3.5f;

    public float chaseDistance = 10f;

    public float attackDistance = 2f;

    public float attackCooldown = 2f;

    public float attackDelay = 1.5f;

    private bool isAttacking;

    private float lastAttackTime;

    private ZombieHealth zombieHealth;


    void Start()
    {
        zombieHealth = GetComponent<ZombieHealth>();
        zombieHealth.OnZombieDeath += HandleZombieDeath;
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = zombieSpeed;
        lastAttackTime = -attackCooldown;
        animator = GetComponent<Animator>();
    }


    private void HandleZombieDeath()
    {
        // Chuyển sang trạng thái Dead
        currentState = ZombieState.Dead;

        // Tắt NavMeshAgent để zombie không di chuyển
        navAgent.isStopped = true;

        // Kích hoạt animation chết
        animator.SetBool("IsDead", true);

        // Tắt collider để ngăn chặn va chạm nếu cần
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;

        // Ngăn Update logic khác
        enabled = false;

        StartCoroutine(RemoveZombieAfterDelay(5f));
    }

    private IEnumerator RemoveZombieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (currentState == ZombieState.Dead)
            return;
        animator.SetFloat("MoveSpeed", navAgent.velocity.magnitude);

        switch (currentState)
        {
            // Các logic khác như Idle, Chase, Attack
        }

        switch (currentState)
        {

            case ZombieState.Idle:
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsAttacking", false);
                if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
                    currentState = ZombieState.Chase;
                break;
                case ZombieState.Chase:
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsAttacking", false);
                navAgent.SetDestination(player.position);
                if(Vector3.Distance(transform.position, player.position) <= attackDistance)
                    currentState = ZombieState.Attack;
                break;
                case ZombieState.Attack:
                animator.SetBool("IsAttacking", true);

                navAgent.SetDestination(transform.position);
                if(!isAttacking && Time.time - lastAttackTime >= attackCooldown)
                {
                    StartCoroutine(AttackWithDelay());
                    Debug.Log("Attack player");
                    //Blood Screen effect

                }
                if (Vector3.Distance(transform.position, player.position) > attackDistance)
                    currentState = ZombieState.Chase;
                break;
            case ZombieState.Dead:
                //Anim
                Debug.Log("Dead");
                break;
        }
    }

   

    public void TakeDamage()
    {

    }

    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackDelay);

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            // Gây sát thương lên nhân vật
            var playerVitals = player.GetComponent<PlayerVitals>();
            if (playerVitals != null)
            {
                HealthEventData damageData = new HealthEventData(-20f); // Sát thương -20
                playerVitals.Entity.ChangeHealth.Try(damageData);
            }
        }

        isAttacking = false;
        lastAttackTime = Time.time;
    }
}
