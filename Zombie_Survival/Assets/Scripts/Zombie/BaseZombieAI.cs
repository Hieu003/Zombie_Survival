using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using HQFPSWeapons;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public abstract class BaseZombieAI : MonoBehaviour
{
    public enum ZombieState { Idle, Walk, Chase, Attack, Dead }
    public ZombieState currentState = ZombieState.Idle;

    public NavMeshAgent navAgent;
    protected Animator animator;
    public Transform player;

    [Header("Audio")]
    public AudioClip[] idleSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] hurtSound;
    public AudioClip[] dieSound;
    private AudioSource audioSource;

    [Header("Item Drop")]
    [SerializeField] private ItemDropRates itemDropRates; // Tham chiếu đến scriptable object ItemDropRates
    [SerializeField][Range(0f, 100f)] private float dropChance = 100f;
    private static HashSet<string> droppedGunPrefabs = new HashSet<string>();

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
    [SerializeField] protected float hearingDistance = 1000f;

    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Đảm bảo Audio Source có Spatial Blend = 3D
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
            Debug.Log("Khong thay nguoi choi");
        }
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

        // Phát âm thanh idle khi bắt đầu
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
            StopIdleSound(); // Dừng âm thanh idle khi chuyển trạng thái
        }
        // Chuyển sang Walk nếu hết thời gian Idle
        else if (Time.time - stateStartTime >= idleDuration)
        {
            currentState = ZombieState.Walk;
            stateStartTime = Time.time;
            StopIdleSound(); // Dừng âm thanh idle khi chuyển trạng thái
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
            navAgent.ResetPath();
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

        if (navAgent != null)
        {
            navAgent.speed = speed;
            navAgent.SetDestination(player.position);
            navAgent.stoppingDistance = attackDistance;
        }

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

        // Phát âm thanh tấn công
        PlayRandomSound(attackSounds);

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

        // Vô hiệu hóa collider ngay lập tức
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;
        GameManager.instance.currentScore += 1;

        TryDropItem();

        OnZombieDeath?.Invoke();
        StartCoroutine(ReleaseWithDelay(5f));
    }

    private IEnumerator ReleaseWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Trả object về pool thay vì hủy
        PoolableObject poolableObject = GetComponent<PoolableObject>();
        if (poolableObject != null)
        {
            PoolingManager.Instance.ReleaseObject(poolableObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance); // Phạm vi tấn công
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
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsPatrolling", false);
            animator.SetBool("IsDead", false);
        }

        // Phát âm thanh idle khi reset về trạng thái Idle
        PlayIdleSound();
    }

    public virtual void HearSound(Vector3 soundPosition, float loudness)
    {
        if (currentState == ZombieState.Dead) return;

        float distanceToSound = Vector3.Distance(transform.position, soundPosition);

        if (distanceToSound <= hearingDistance * loudness)
        {
            // Kích hoạt trạng thái Chase khi nghe thấy âm thanh
            if (currentState != ZombieState.Attack)
            {
                currentState = ZombieState.Chase;
                stateStartTime = Time.time;
            }

            // Chạy ChaseBehavior() ngay lập tức để phản ứng nhanh hơn
            ChaseBehavior();
        }
    }

    private void TryDropItem()
    {
        // Tạo một số ngẫu nhiên từ 0.0 đến 100.0
        float randomValue = Random.Range(0f, 100f);

        if (randomValue <= dropChance)
        {
            // Tính tổng trọng số
            float totalWeight = 0;
            foreach (ItemDropRates.DropRate dropRate in itemDropRates.dropRates)
            {
                if (dropRate.itemPrefab.GetComponent<Gun>() == null || !droppedGunPrefabs.Contains(dropRate.itemPrefab.name))
                    totalWeight += dropRate.dropWeight;
            }

            // Chọn ngẫu nhiên một item dựa trên trọng số
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
                            // Lấy item từ pool
                          
                          
                            PoolableObject item = PoolingManager.Instance.GetObject(itemPrefab.GetInstanceID().ToString(), transform.position, Quaternion.identity);
                            if (item == null)
                            {
                                Debug.LogError("Failed to get item from pool: " + itemPrefab.name);
                            }

                            // Nếu là súng, đánh dấu đã rơi
                            if (itemPrefab.GetComponent<Gun>() != null)
                            {
                                droppedGunPrefabs.Add(itemPrefab.name);
                            }
                        }
                        break; // Đã tìm thấy item để rơi, thoát vòng lặp
                    }
                }
            }
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

    // Dừng phát âm thanh idle
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