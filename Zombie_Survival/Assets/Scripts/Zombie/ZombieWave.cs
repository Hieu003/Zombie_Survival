using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQFPSWeapons;
using UnityEngine.UI;

public class ZombieWave : MonoBehaviour
{
    private const string DefaultDifficulty = "Easy";
    private const string HardDifficulty = "Hard";
    private const string SpawnAreaTag = "SpawnArea";

    [SerializeField] private GameObject[] zombiePrefabs;
    [SerializeField] private GameObject[] itemPickupPrefabs;
    [SerializeField] private float timeBetweenWaves = 15f;
    [SerializeField] private int initialZombiePerWave = 4;
    [SerializeField] private int maxZombies = 100;
    [SerializeField] private float maxSpawnDistance = 10f;
    [SerializeField] private Text waveNumberText;
    [SerializeField] private Text waveTimerText;
    [SerializeField] private Text zombieCountText;

    private float waveTimer = 0f;
    private int waveNumber = 0;
    private int currentZombieCount = 0;
    private bool waveSpawning = false;
    private int zombieIncrement;
    private int zombiePerWave;
    private bool isFirstWave = true;
    private Collider[] spawnAreaColliders;

    private void Start()
    {
        InitializeDifficultySettings();
        InitializePools();
        InitializeSpawnAreas();
        waveTimerText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (isFirstWave)
        {
            HandleFirstWave();
        }
        else
        {
            HandleSubsequentWaves();
        }

        zombieCountText.text = $"Zombie: {currentZombieCount}";
    }

    private void InitializeDifficultySettings()
    {
        string difficulty = PlayerPrefs.GetString("Difficulty", DefaultDifficulty);

        if (difficulty == HardDifficulty)
        {
            timeBetweenWaves = 10f;
            zombieIncrement = 5;
            Debug.Log("Hard difficulty loaded");
        }
        else
        {
            zombieIncrement = 3;
            Debug.Log("Easy difficulty loaded");
        }

        zombiePerWave = initialZombiePerWave;
    }

    private void InitializePools()
    {
        foreach (GameObject prefab in zombiePrefabs)
        {
            PoolingManager.Instance.CreatePool(prefab, 10, 30, true, prefab.GetInstanceID().ToString());
        }

        foreach (GameObject itemPrefab in itemPickupPrefabs)
        {
            PoolingManager.Instance.CreatePool(itemPrefab, 5, 10, true, itemPrefab.GetInstanceID().ToString());
        }
    }

    private void InitializeSpawnAreas()
    {
        GameObject[] spawnAreaObjects = GameObject.FindGameObjectsWithTag(SpawnAreaTag);
        spawnAreaColliders = new Collider[spawnAreaObjects.Length];
        for (int i = 0; i < spawnAreaObjects.Length; i++)
        {
            spawnAreaColliders[i] = spawnAreaObjects[i].GetComponent<Collider>();
        }
    }

    private void HandleFirstWave()
    {
        waveTimer += Time.deltaTime;
        int intValue = Mathf.RoundToInt(waveTimer);
        waveTimerText.text = $"A new wave will arrive in {timeBetweenWaves - intValue} s";

        if (waveTimer >= timeBetweenWaves)
        {
            StartNewWave();
            isFirstWave = false;
            waveTimerText.gameObject.SetActive(false);
        }
    }

    private void HandleSubsequentWaves()
    {
        if (waveSpawning)
        {
            waveTimerText.gameObject.SetActive(false);
            return;
        }

        if (currentZombieCount == 0)
        {
            waveTimer += Time.deltaTime;
            int intValue = Mathf.RoundToInt(waveTimer);

            waveTimerText.gameObject.SetActive(true);
            waveTimerText.text = $"A new wave will arrive in {timeBetweenWaves - intValue} s";

            if (waveTimer >= timeBetweenWaves)
            {
                StartNewWave();
            }
        }
        else
        {
            waveTimer = 0f;
            waveTimerText.gameObject.SetActive(false);
        }
    }

    private void StartNewWave()
    {
        waveTimer = 0f;
        waveNumber++;
        waveNumberText.text = $"Wave: {waveNumber}";

        if (waveNumber > 1)
        {
            zombiePerWave = Mathf.Min(zombiePerWave + zombieIncrement, maxZombies);
        }

        currentZombieCount = 0;
        waveSpawning = true;

        SpawnZombies();

        waveSpawning = false;
    }

    private void SpawnZombies()
    {
        for (int i = 0; i < zombiePerWave; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            GameObject randomZombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
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
            Collider spawnAreaCollider = spawnAreaColliders[Random.Range(0, spawnAreaColliders.Length)];
            spawnPosition = GetRandomPointInsideCollider(spawnAreaCollider);
            validPositionFound = true;
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

        Vector3 center = collider.bounds.center;
        Vector3 direction = point - center;
        if (direction.magnitude > maxSpawnDistance)
        {
            direction = direction.normalized * maxSpawnDistance;
            point = center + direction;
        }

        point.y = collider.transform.position.y;

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
