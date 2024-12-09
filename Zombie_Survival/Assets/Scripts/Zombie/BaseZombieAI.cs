using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using HQFPSWeapons;

public abstract class BaseZombieAI : MonoBehaviour
{
    public enum ZombieState { Idle, Walk, Chase, Attack, Dead }
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
    [SerializeField] protected float idleDuration = 5f; 
    [SerializeField] protected float walkDuration = 10f; 
    [SerializeField] protected float walkSpeedMultiplier = 0.5f;
    protected float stateStartTime;

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

        // Bắt đầu với trạng thái Idle
        currentState = ZombieState.Idle;
        stateStartTime = Time.time;
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
            case ZombieState.Walk: // Thêm case Walk
                WalkBehavior();
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
       
        if (!(this is TankZombieAI))
        {
            animator.SetBool("IsPatrolling", false);
        }
        // Chuyển sang Chase nếu thấy người chơi
        if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
        {
            currentState = ZombieState.Chase;
            stateStartTime = Time.time;
        }
        // Chuyển sang Walk nếu hết thời gian Idle
        else if (Time.time - stateStartTime >= idleDuration)
        {
            currentState = ZombieState.Walk;
            stateStartTime = Time.time;
        }
    }
    protected virtual void WalkBehavior()
    {
        if (!(this is TankZombieAI))
        {
            animator.SetBool("IsPatrolling", true);
        }
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsWalking", false);

        // Thiết lập tốc độ Walk
        if (navAgent != null)
            navAgent.speed = speed * walkSpeedMultiplier;

        if (!navAgent.hasPath)
        {
            navAgent.SetDestination(GetRandomNavMeshLocation());
        }

        // Chuyển sang Chase nếu thấy người chơi
        if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
        {
            currentState = ZombieState.Chase;
            stateStartTime = Time.time;
        }
        // Chuyển sang Idle nếu hết thời gian Walk
        else if (Time.time - stateStartTime >= walkDuration)
        {
            currentState = ZombieState.Idle;
            stateStartTime = Time.time;
            navAgent.ResetPath();
        }
    }

    protected virtual void ChaseBehavior()
    {
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsWalking", true);
        if (!(this is TankZombieAI))
        {
            animator.SetBool("IsPatrolling", false);
        }

        // Thiết lập lại tốc độ bình thường
        if (navAgent != null)
            navAgent.speed = speed;

        if (navAgent != null)
        {
            navAgent.SetDestination(player.position);
            navAgent.stoppingDistance = attackDistance;
        }

        // Chuyển sang Attack nếu trong khoảng cách Attack
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            currentState = ZombieState.Attack;
            stateStartTime = Time.time;
        }

    }


    protected virtual void AttackBehavior()
    {

        animator.SetBool("IsAttacking", true);
        animator.SetBool("IsWalking", false);
        if (!(this is TankZombieAI))
        {
            animator.SetBool("IsPatrolling", false);
        }

        // Dừng zombie tại vị trí hiện tại để tấn công
        if (navAgent != null)
            navAgent.SetDestination(player.position);

        // Chỉ gây sát thương nếu không đang tấn công
        if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            StartCoroutine(AttackWithDelay());

        // Quay lại Chase nếu người chơi thoát khỏi attackDistance
        if (Vector3.Distance(transform.position, player.position) > attackDistance + 0.1)
        {
            navAgent.stoppingDistance = 0f; // Reset khoảng cách dừng
            currentState = ZombieState.Chase;

        }
    }

    public Vector3 GetRandomNavMeshLocation()
    {
        // Lấy NavMesh triangulation
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        // Chọn một tam giác ngẫu nhiên từ NavMesh
        int randomIndex = Random.Range(0, navMeshData.indices.Length / 3);

        // Lấy ba đỉnh của tam giác được chọn
        Vector3 vertex1 = navMeshData.vertices[navMeshData.indices[randomIndex * 3]];
        Vector3 vertex2 = navMeshData.vertices[navMeshData.indices[randomIndex * 3 + 1]];
        Vector3 vertex3 = navMeshData.vertices[navMeshData.indices[randomIndex * 3 + 2]];

        // Tính toán điểm ngẫu nhiên trong tam giác
        Vector3 randomPointInTriangle = (vertex1 + vertex2 + vertex3) / 3f; // Điểm trọng tâm
        randomPointInTriangle += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)); // Thêm offset ngẫu nhiên

        // Đặt lại vị trí y để phù hợp với mặt đất
        randomPointInTriangle.y = transform.position.y;

        return randomPointInTriangle;
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



