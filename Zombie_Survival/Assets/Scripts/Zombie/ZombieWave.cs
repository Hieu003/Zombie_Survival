using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQFPSWeapons;

public class ZombieWave : MonoBehaviour
{
    public Transform[] spawnPoints;
    public float timeBetweenWave = 10f;
    private float waveTimer = 0f;
    private int waveNumber = 1;
    public int zombiePerWave = 4;
    protected ZombieHealth zombieHealth;
    public float timeBetweenSpawns = 0.5f; // Thời gian giữa mỗi lần spawn nhỏ


    private void Update()
    {
        if (waveNumber == 10)
            return;

        waveTimer += Time.deltaTime;

        if (waveTimer >= timeBetweenWave)
        {
            StartNewWave();
        }
    }

    void StartNewWave()
    {
        waveTimer = 0f;
        zombiePerWave += 2;
        StartCoroutine(SpawnZombies());
        waveNumber++;
    }

    IEnumerator SpawnZombies()
    {
        float minDistance = 4f;

        for (int i = 0; i < zombiePerWave; i++)
        {
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomSpawnIndex];

            GameObject randomZombiePrefab = ZombiePool.Instance.zombiePrefabs[Random.Range(0, ZombiePool.Instance.zombiePrefabs.Length)];

            GameObject zombie = ZombiePool.Instance.GetZombieFromPool(randomZombiePrefab.name);

            if (zombie != null)
            {
                Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * minDistance;
                spawnPosition.y = spawnPoint.position.y;
                zombie.transform.position = spawnPosition;
                zombie.transform.rotation = spawnPoint.rotation;

                BaseZombieAI zombieAI = zombie.GetComponent<BaseZombieAI>();
                if (zombieAI != null)
                {
                    zombieAI.ResetState();
                }
                ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>();
                if (zombieHealth != null)
                {
                    zombieHealth.ResetHealth();
                }
            }

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }
}