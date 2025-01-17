using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using HQFPSWeapons;
using UnityEngine.SceneManagement;
using System.Linq;

public abstract class BaseZombieAI : MonoBehaviour
{
    public enum ZombieState { Idle, Walk, Chase, Attack, Dead }
    public ZombieState currentState = ZombieState.Idle;

    public NavMeshAgent navAgent;
    protected Animator animator;
    public Transform player;
    public float damage = 10f;
    public float attackForce = 100f;

    [Header("Audio")]
    public AudioClip[] idleSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] hurtSound;
    public AudioClip[] dieSound;
    private AudioSource audioSource;

    [Header("Item Drop")]
    [SerializeField] private ItemDropRates itemDropRates;
    [SerializeField][Range(0f, 100f)] private float dropChance = 100f;
    private static HashSet<string> droppedGunPrefabs = new HashSet<string>();

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
    [SerializeField] protected float hearingDistance = 1000f;

    protected virtual void Start()
    {
        CacheComponents();
        InitializeZombie();
    }

    private void CacheComponents()
    {
        audioSource = GetComponent<AudioSource>();
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        zombieHealth = GetComponent<ZombieHealth>();
    }

    private void InitializeZombie()
    {
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1f;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.Log("Player not found");
        }

        if (zombieHealth != null)
        {
            zombieHealth.OnZombieDeath += HandleZombieDeath;
        }

        lastAttackTime = -attackCooldown;

        if (navAgent != null)
        {
            navAgent.speed = speed;
        }

        currentState = ZombieState.Idle;
        stateStartTime = Time.time;

        PlayIdleSound();
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
            case ZombieState.Walk:
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
        SetAnimatorBools(false, false, false);

        if (Vector3.SqrMagnitude(transform.position - player.position) <= chaseDistance * chaseDistance)
        {
            TransitionToState(ZombieState.Chase);
        }
        else if (Time.time - stateStartTime >= idleDuration)
        {
            TransitionToState(ZombieState.Walk);
        }
    }

    protected virtual void WalkBehavior()
    {
        SetAnimatorBools(false, false, true);

        if (navAgent != null)
        {
            navAgent.speed = speed * walkSpeedMultiplier;

            if (!navAgent.hasPath)
            {
                navAgent.SetDestination(GetRandomNavMeshLocation());
            }
        }

        if (Vector3.SqrMagnitude(transform.position - player.position) <= chaseDistance * chaseDistance)
        {
            TransitionToState(ZombieState.Chase);
        }
        else if (Time.time - stateStartTime >= walkDuration)
        {
            TransitionToState(ZombieState.Idle);
        }
    }

    protected virtual void ChaseBehavior()
    {
        SetAnimatorBools(true, false, false);

        if (navAgent != null)
        {
            navAgent.speed = speed;
            navAgent.SetDestination(player.position);
            navAgent.stoppingDistance = attackDistance;
        }

        if (Vector3.SqrMagnitude(transform.position - player.position) <= attackDistance * attackDistance)
        {
            TransitionToState(ZombieState.Attack);
        }
    }

    protected virtual void AttackBehavior()
    {
        SetAnimatorBools(false, true, false);

        if (navAgent != null)
        {
            navAgent.SetDestination(player.position);
        }

        if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            StartCoroutine(AttackWithDelay());
        }

        if (Vector3.SqrMagnitude(transform.position - player.position) > attackDistance * attackDistance)
        {
            navAgent.stoppingDistance = 0f;
            TransitionToState(ZombieState.Chase);
        }
    }

    private void SetAnimatorBools(bool isWalking, bool isAttacking, bool isPatrolling)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsAttacking", isAttacking);
            if (!(this is TankZombieAI))
            {
                animator.SetBool("IsPatrolling", isPatrolling);
            }
        }
    }

    private void TransitionToState(ZombieState newState)
    {
        currentState = newState;
        stateStartTime = Time.time;
        StopIdleSound();
    }

    public Vector3 GetRandomNavMeshLocation()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        int randomIndex = Random.Range(0, navMeshData.indices.Length / 3);

        Vector3 vertex1 = navMeshData.vertices[navMeshData.indices[randomIndex * 3]];
        Vector3 vertex2 = navMeshData.vertices[navMeshData.indices[randomIndex * 3 + 1]];
        Vector3 vertex3 = navMeshData.vertices[navMeshData.indices[randomIndex * 3 + 2]];

        Vector3 randomPointInTriangle = (vertex1 + vertex2 + vertex3) / 3f;
        randomPointInTriangle += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        randomPointInTriangle.y = transform.position.y;

        return randomPointInTriangle;
    }

    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;
        PlayRandomSound(attackSounds);
        yield return new WaitForSeconds(attackDelay);

        if (Vector3.SqrMagnitude(transform.position - player.position) <= attackDistance * attackDistance)
        {
            PerformAttack();
        }

        isAttacking = false;
        lastAttackTime = Time.time;
    }

    protected virtual void HandleZombieDeath()
    {
        currentState = ZombieState.Dead;

        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        DisableColliders();
        GameManager.instance.currentScore += 1;
        TryDropItem();
        OnZombieDeath?.Invoke();
        StartCoroutine(ReleaseWithDelay(5f));
    }

    private void DisableColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
    }

    private IEnumerator ReleaseWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        PoolableObject poolableObject = GetComponent<PoolableObject>();
        if (poolableObject != null)
        {
            PoolingManager.Instance.ReleaseObject(poolableObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    protected abstract void PerformAttack();

    public void ResetState()
    {
        currentState = ZombieState.Idle;
        stateStartTime = Time.time;
        isAttacking = false;
        lastAttackTime = -attackCooldown;

        if (navAgent != null)
        {
            navAgent.isStopped = false;
            navAgent.speed = speed;
            navAgent.ResetPath();
        }

        SetAnimatorBools(false, false, false);
        PlayIdleSound();
    }

    public virtual void HearSound(Vector3 soundPosition, float loudness)
    {
        if (currentState == ZombieState.Dead) return;

        float distanceToSound = Vector3.Distance(transform.position, soundPosition);

        if (distanceToSound <= hearingDistance * loudness)
        {
            if (currentState != ZombieState.Attack)
            {
                TransitionToState(ZombieState.Chase);
            }

            ChaseBehavior();
        }
    }

    private void TryDropItem()
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= dropChance)
        {
            float totalWeight = itemDropRates.dropRates
                .Where(dropRate => dropRate.itemPrefab.GetComponent<Gun>() == null || !droppedGunPrefabs.Contains(dropRate.itemPrefab.name))
                .Sum(dropRate => dropRate.dropWeight);

            float randomWeightValue = Random.Range(0f, totalWeight);
            float currentWeight = 0;

            foreach (ItemDropRates.DropRate dropRate in itemDropRates.dropRates)
            {
                if (dropRate.itemPrefab.GetComponent<Gun>() == null || !droppedGunPrefabs.Contains(dropRate.itemPrefab.name))
                {
                    currentWeight += dropRate.dropWeight;
                    if (randomWeightValue <= currentWeight)
                    {
                        GameObject itemPrefab = dropRate.itemPrefab;

                        if (itemPrefab != null)
                        {
                            PoolableObject item = PoolingManager.Instance.GetObject(itemPrefab.GetInstanceID().ToString(), transform.position, Quaternion.identity);
                            if (item == null)
                            {
                                Debug.LogError("Failed to get item from pool: " + itemPrefab.name);
                            }

                            if (itemPrefab.GetComponent<Gun>() != null)
                            {
                                droppedGunPrefabs.Add(itemPrefab.name);
                            }
                        }
                        break;
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnableColliders();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void EnableColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        droppedGunPrefabs.Clear();
    }

    public event System.Action OnZombieDeath;

    public void PlayHurtSound()
    {
        PlayRandomSound(hurtSound);
    }

    public void PlayDieSound()
    {
        PlayRandomSound(dieSound);
    }

    public void PlayIdleSound()
    {
        if (audioSource != null && idleSounds.Length > 0)
        {
            audioSource.clip = idleSounds[Random.Range(0, idleSounds.Length)];
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopIdleSound()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip != null && idleSounds.Contains(audioSource.clip))
        {
            audioSource.Stop();
        }
    }

    private void PlayRandomSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource != null && clips.Length > 0)
        {
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}
