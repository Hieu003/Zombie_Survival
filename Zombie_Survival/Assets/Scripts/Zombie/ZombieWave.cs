using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQFPSWeapons;
using UnityEngine.UI;

public class ZombieWave : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] zombiePrefabs;
    public float timeBetweenWaves = 10f;
    private float waveTimer = 0f;
    private int waveNumber = 0;
    public int initialZombiePerWave = 4; // Số lượng zombie khởi đầu
    public int maxZombies = 100; // Giới hạn số lượng zombie tối đa
    public float spawnRadius = 50f; // Bán kính spawn
    public float timeBetweenSpawns = 0.5f;

    public Text WaveNumberText;
    public Text WaveTimerText;
    public Text ZombieCountText; // Thêm Text để hiển thị số lượng zombie còn lại

    private string difficulty;
    private int currentZombieCount = 0; // Số lượng zombie hiện tại
    private bool waveSpawning = false;
    private int zombieIncrement; // Số lượng zombie tăng thêm mỗi wave
    private int zombiePerWave;

    private bool isFirstWave = true;

    private void Start()
    {
        difficulty = PlayerPrefs.GetString("Difficulty", "Easy");

        if (difficulty == "Hard")
        {
            timeBetweenWaves = 5f;
            zombieIncrement = 5;
            zombiePerWave = initialZombiePerWave + 3;
            Debug.Log("Hard difficulty loaded");
        }
        else
        {
            zombieIncrement = 3;
            zombiePerWave = initialZombiePerWave;
            Debug.Log("Easy difficulty loaded");
        }

        foreach (GameObject prefab in zombiePrefabs)
        {
            PoolingManager.Instance.CreatePool(prefab, 10, 30, true, prefab.GetInstanceID().ToString());
        }
        WaveTimerText.gameObject.SetActive(true);
        waveTimer = 0f;
    }

    private void Update()
    {
        if (isFirstWave)
        {
            // Nếu là wave đầu tiên, hiển thị thông báo và đếm thời gian
            waveTimer += Time.deltaTime;
            int intValue = Mathf.RoundToInt(waveTimer);
            WaveTimerText.text = "A new wave will arrive in " + (timeBetweenWaves - intValue).ToString() + " s";

            if (waveTimer >= timeBetweenWaves)
            {
                // Bắt đầu wave 1 sau khi đếm xong thời gian
                StartNewWave();
                isFirstWave = false; // Không còn là wave đầu tiên nữa
                WaveTimerText.gameObject.SetActive(false); // Ẩn WaveTimerText
            }
        }
        else
        {
            // Các wave sau đó
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
        zombiePerWave = Mathf.Min(zombiePerWave + zombieIncrement, maxZombies);

        currentZombieCount = zombiePerWave;
        waveSpawning = true;
        StartCoroutine(SpawnZombies());
    }

    IEnumerator SpawnZombies()
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

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        waveSpawning = false;
    }
    private void OnZombieDied()
    {
        currentZombieCount--;
    }
}