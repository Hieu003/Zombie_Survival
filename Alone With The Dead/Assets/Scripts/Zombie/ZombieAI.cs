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

    public float attackDelay = 2f;

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
                Debug.Log("Attack player");
                if (Vector3.Distance(transform.position, player.position) > attackDistance)
                    currentState = ZombieState.Chase;
                break;
            case ZombieState.Dead:
                //Anim
                Debug.Log("Dead");
                break;
        }
    }
}
