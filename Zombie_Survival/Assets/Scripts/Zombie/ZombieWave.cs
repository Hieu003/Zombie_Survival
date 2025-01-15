using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQFPSWeapons;
using UnityEngine.UI;

public class ZombieWave : MonoBehaviour
{
    public string spawnAreaTag = "SpawnArea";
    public GameObject[] zombiePrefabs;
    public GameObject[] itemPickupPrefabs;
    private float timeBetweenWaves = 15f;
    private float waveTimer = 0f;
    private int waveNumber = 0;
    public int initialZombiePerWave = 4; // Số lượng zombie khởi đầu
    public int maxZombies = 100; // Giới hạn số lượng zombie tối đa
    
    public float maxSpawnDistance = 10f;

    public Text WaveNumberText;
    public Text WaveTimerText;
    public Text ZombieCountText;

    private string difficulty;
    private int currentZombieCount = 0;
    private bool waveSpawning = false;
    private int zombieIncrement;
    private int zombiePerWave;

    private bool isFirstWave = true;
    private Collider[] spawnAreaColliders;

    private void Start()
    {
        difficulty = PlayerPrefs.GetString("Difficulty", "Easy");

        // Khác biệt chính giữa Easy và Hard: số lượng zombie tăng thêm mỗi wave
        if (difficulty == "Hard")
        {
            timeBetweenWaves = 10f;
            zombieIncrement = 5;
            zombiePerWave = initialZombiePerWave;
            Debug.Log("Hard difficulty loaded");
        }
        else
        {
            zombieIncrement = 3;
            zombiePerWave = initialZombiePerWave;
            Debug.Log("Easy difficulty loaded");
        }

        // Tạo pool cho zombie và item pickup
        foreach (GameObject prefab in zombiePrefabs)
        {
            PoolingManager.Instance.CreatePool(prefab, 10, 30, true, prefab.GetInstanceID().ToString());
        }

        foreach (GameObject itemPrefab in itemPickupPrefabs)
        {
            PoolingManager.Instance.CreatePool(itemPrefab, 5, 10, true, itemPrefab.GetInstanceID().ToString());
        }

        WaveTimerText.gameObject.SetActive(true);
        waveTimer = 0f;


        // Lấy tất cả các collider có tag "SpawnArea"
        GameObject[] spawnAreaObjects = GameObject.FindGameObjectsWithTag(spawnAreaTag);
        spawnAreaColliders = new Collider[spawnAreaObjects.Length];
        for (int i = 0; i < spawnAreaObjects.Length; i++)
        {
            spawnAreaColliders[i] = spawnAreaObjects[i].GetComponent<Collider>();
        }
    }

    private void Update()
    {
        if (isFirstWave)
        {
            waveTimer += Time.deltaTime;
            int intValue = Mathf.RoundToInt(waveTimer);
            WaveTimerText.text = "A new wave will arrive in " + (timeBetweenWaves - intValue).ToString() + " s";

            if (waveTimer >= timeBetweenWaves)
            {
                StartNewWave();
                isFirstWave = false;
                WaveTimerText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (waveSpawning)
            {
                WaveTimerText.gameObject.SetActive(false);
                return;
            }

            if (currentZombieCount == 0)
            {
                waveTimer += Time.deltaTime;
                int intValue = Mathf.RoundToInt(waveTimer);

                WaveTimerText.gameObject.SetActive(true);
                WaveTimerText.text = "A new wave will arrive in " + (timeBetweenWaves - intValue).ToString() + " s";

                if (waveTimer >= timeBetweenWaves)
                {
                    StartNewWave();
                }
            }
            else
            {
                waveTimer = 0f;
                WaveTimerText.gameObject.SetActive(false);
            }
        }

        ZombieCountText.text = "Zombie: " + currentZombieCount.ToString();
    }

    void StartNewWave()
    {
        waveTimer = 0f;
        waveNumber++;
        WaveNumberText.text = "Wave: " + waveNumber.ToString();

        // Tăng số lượng zombie mỗi wave dựa vào độ khó, nhưng không vượt quá maxZombies
        if (waveNumber > 1)
        {
            zombiePerWave = Mathf.Min(zombiePerWave + zombieIncrement, maxZombies);
        }

        currentZombieCount = 0;
        waveSpawning = true;

        // Spawn tất cả zombie cùng lúc
        SpawnZombies();

        waveSpawning = false; // Đặt waveSpawning = false ngay sau khi spawn xong
    }

    void SpawnZombies()
    {
        for (int i = 0; i < zombiePerWave; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            // Lấy ngẫu nhiên một prefab zombie từ mảng
            GameObject randomZombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];

            // Lấy zombie từ pool
            PoolableObject zombie = PoolingManager.Instance.GetObject(randomZombiePrefab.GetInstanceID().ToString(), spawnPosition, Quaternion.identity);

            if (zombie != null)
            {
                currentZombieCount++;
                zombie.transform.position = spawnPosition;
                zombie.transform.rotation = Quaternion.identity;

                BaseZombieAI zombieAI = zombie.GetComponent<BaseZombieAI>();
                if (zombieAI != null)
                {
                    zombieAI.ResetState();
                    zombieAI.OnZombieDeath += OnZombieDied;
                }

                ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>();
                if (zombieHealth != null)
                {
                    zombieHealth.ResetHealth();
                }
            }
        }
    }


    private Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        while (!validPositionFound)
        {
            // Chọn ngẫu nhiên một collider từ mảng spawnAreaColliders
            Collider spawnAreaCollider = spawnAreaColliders[Random.Range(0, spawnAreaColliders.Length)];

            // Lấy một điểm ngẫu nhiên trong collider đó
            spawnPosition = GetRandomPointInsideCollider(spawnAreaCollider);

            // Kiểm tra xem vị trí spawn có hợp lệ không (nếu cần)
            // Ở đây, bạn có thể thêm các điều kiện kiểm tra khác nếu cần
            // Ví dụ: kiểm tra khoảng cách tối thiểu từ người chơi, kiểm tra NavMesh, v.v.

            validPositionFound = true; // Tạm thời coi vị trí là hợp lệ, bạn có thể thay đổi điều kiện này
        }

        return spawnPosition;
    }

    private Vector3 GetRandomPointInsideCollider(Collider collider)
    {
        Vector3 extents = collider.bounds.extents;
        Vector3 point = new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        );

        point = collider.transform.TransformPoint(point);

        // Đảm bảo điểm spawn không quá xa so với tâm của collider
        Vector3 center = collider.bounds.center;
        Vector3 direction = point - center;
        if (direction.magnitude > maxSpawnDistance)
        {
            direction = direction.normalized * maxSpawnDistance;
            point = center + direction;
        }

        point.y = collider.transform.position.y; // Giữ nguyên độ cao

        return point;
    }

    private void OnZombieDied()
    {
        currentZombieCount--;
        if (currentZombieCount < 0)
        {
            currentZombieCount = 0;
        }
    }
}