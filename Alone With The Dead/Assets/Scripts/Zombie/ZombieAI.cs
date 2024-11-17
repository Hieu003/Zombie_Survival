using HQFPSWeapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public NavMeshAgent navAgent;

    public enum ZombieState { Idle, Chase, Attack, Dead};

    public ZombieState currentState = ZombieState.Idle;

    public Transform player;

    public float chaseDistance = 10f;

    public float attackDistance = 2f;

    public float attackCooldown = 2f;

    public float attackDelay = 1.5f;

    private bool isAttacking;

    private float lastAttackTime;

     void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        lastAttackTime = -attackCooldown;
    }


    private void Update()
    {
        switch(currentState)
        {

            case ZombieState.Idle:
                //Anim
                if(Vector3.Distance(transform.position, player.position) <= chaseDistance)
                    currentState = ZombieState.Chase;
                break;
                case ZombieState.Chase:
                //Anim
                navAgent.SetDestination(player.position);
                if(Vector3.Distance(transform.position, player.position) <= attackDistance)
                    currentState = ZombieState.Attack;
                break;
                case ZombieState.Attack:
                //Anim
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
