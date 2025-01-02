using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQFPSWeapons;
using UnityEngine.UI;

public class ZombieWave : MonoBehaviour
{
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 10f; // Giá trị mặc định cho Easy
    private float waveTimer = 0f;
    private int waveNumber = 0;
    public int zombiePerWave = 4; // Giá trị mặc định cho Easy
    protected ZombieHealth zombieHealth;
    public float timeBetweenSpawns = 2f; // Thời gian giữa mỗi lần spawn nhỏ

    public Text WaveNumber;
    public Text WaveTimer;

    private void Start()
    {
        // Lấy độ khó từ PlayerPrefs
        string difficulty = PlayerPrefs.GetString("Difficulty", "Easy"); // "Easy" là giá trị mặc định nếu không tìm thấy

        // Thiết lập các giá trị dựa trên độ khó
        if (difficulty == "Hard")
        {
            timeBetweenWaves = 5f;
            zombiePerWave = 7;
            Debug.Log("Hard difficulty loaded");
        }
        else
        {
            // Mặc định là Easy, đã được gán giá trị ở trên
            Debug.Log("Easy difficulty loaded");
        }
    }

    private void Update()
    {
        if (waveNumber == 10)
            return;

        waveTimer += Time.deltaTime;
        int intValue = Mathf.RoundToInt(waveTimer);
        WaveTimer.text = intValue.ToString();

        if (waveTimer >= timeBetweenWaves)
        {
            StartNewWave();
        }
    }

    void StartNewWave()
    {
        waveTimer = 0f;
        // zombiePerWave += 2; //Cái này sẽ thay đổi liên tục sau mỗi đợt nên bỏ ra ngoài vòng if
        StartCoroutine(SpawnZombies());
        waveNumber++;
        WaveNumber.text = waveNumber.ToString();
        // Di chuyển zombiePerWave += 2; ra ngoài if để nó tăng sau mỗi wave, bất kể độ khó
        if (difficulty == "Hard")
        {
            zombiePerWave += 3;
        }
        else
        {
            zombiePerWave += 2;
        }
    }

    IEnumerator SpawnZombies()
    {
        float minDistance = 10f;

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
    private string difficulty;

}