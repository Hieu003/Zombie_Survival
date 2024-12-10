using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieWave : MonoBehaviour
{
    public GameObject[] zombiePrefabs;

    public Transform[] spawnPoints;


    public float timeBetweenWave = 10f;

    [SerializeField] private float waveTimer = 0f;

    private int waveNumber = 1;

    public int zombiePerWave = 4;



    private void Update()
    {
        if (waveNumber == 10)
            return;
        waveTimer += Time.deltaTime;

        int intValue = Mathf.RoundToInt(waveTimer);

        if(waveTimer >= timeBetweenWave)
        {
            StartNewWave();
        }
    }
    void StartNewWave()
    {
        waveTimer = 0f;

        zombiePerWave += 2;

        float minDistance = 4f;

        for(int i = 0; i < zombiePerWave; i++)
        {
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomSpawnIndex];

            GameObject randomZombiePrefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];

            Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * minDistance;

            spawnPosition.y = spawnPoint.position.y;

            Instantiate(randomZombiePrefab, spawnPosition, spawnPoint.rotation);
        }
        waveNumber++;
    }

}
