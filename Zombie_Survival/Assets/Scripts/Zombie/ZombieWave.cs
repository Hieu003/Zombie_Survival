using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQFPSWeapons;
using UnityEngine.UI;

public class ZombieWave : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] zombiePrefabs;
    public GameObject[] itemPickupPrefabs;
    public float timeBetweenWaves = 10f;
    private float waveTimer = 0f;
    private int waveNumber = 0;
    public int initialZombiePerWave = 4; // Số lượng zombie khởi đầu
    public int maxZombies = 100; // Giới hạn số lượng zombie tối đa
    private float spawnRadius = 0.1f; // Bán kính spawn

    public Text WaveNumberText;
    public Text WaveTimerText;
    public Text ZombieCountText;

    private string difficulty;
    private int currentZombieCount = 0;
    private bool waveSpawning = false;
    private int zombieIncrement;
    private int zombiePerWave;

    private bool isFirstWave = true;

    private void Start()
    {
        difficulty = PlayerPrefs.GetString("Difficulty", "Easy");

        // Khác biệt chính giữa Easy và Hard: số lượng zombie tăng thêm mỗi wave
        if (difficulty == "Hard")
        {
            timeBetweenWaves = 5f;
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
            // Chọn ngẫu nhiên một spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Lấy ngẫu nhiên một prefab zombie từ mảng
            GameObject randomZombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];

            // Tạo vị trí spawn ngẫu nhiên trong bán kính spawnRadius
            Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = spawnPoint.position.y; // Giữ nguyên độ cao

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
                    zombieAI.OnZombieDeath += OnZombieDied; // Đăng ký sự kiện zombie chết
                }

                ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>();
                if (zombieHealth != null)
                {
                    zombieHealth.ResetHealth();
                }
            }
        }
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