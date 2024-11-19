using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using HQFPSWeapons;

public class ZombiePatrolAI : MonoBehaviour
{

    public NavMeshAgent navAgent;

    public enum ZombieState { Patrol, Chase, Attack, Dead };

    public Animator animator;

    public ZombieState currentState = ZombieState.Patrol;

    public Transform player;

    public float zombieSpeed = 3.5f;

    public float chaseDistance = 10f;

    public float attackDistance = 2f;

    public float attackCooldown = 2f;

    public float attackDelay = 1.5f;

    private bool isAttacking;

    private float lastAttackTime;

    private ZombieHealth zombieHealth;

    private bool isMoving = false;


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

     void Update()
    {
       switch(currentState)
        {

            case ZombieState.Patrol:
                if(!isMoving || navAgent.remainingDistance < 0.1f)
                {
                    //Patroll
                }
                if (IsPlayerInRange(chaseDistance))
                    currentState = ZombieState.Chase;
                break;
            case ZombieState.Chase:
                if(IsPlayerInRange(attackDistance))
                    currentState= ZombieState.Attack;
                break;
                case ZombieState.Attack:
                if(!IsPlayerInRange(attackDistance))
                    currentState= ZombieState.Chase;
                break;
            case ZombieState.Dead:
                //Anim
                Debug.Log("Dead");
                break;
        }
        
    }

    private bool IsPlayerInRange(float range)
    {
        return Vector3.Distance(transform.position, player.position) <= range;
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
