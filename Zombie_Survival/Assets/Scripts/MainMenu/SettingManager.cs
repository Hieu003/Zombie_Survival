using UnityEngine;
using UnityEngine.UI;
using HQFPSWeapons;


public class SettingManager : MonoBehaviour
{
    public Button normalDifficultyButton;

    public Button hardDifficultyButton;

    public Button saveButton;

    public Button newGameButton;

    [SerializeField] private float timeBetweenWaves;

    [SerializeField] private int zombiePerWave;

    private void Start()
    {
        LoadSetting();

        normalDifficultyButton.onClick.AddListener(SetNormalDifficulty);
        hardDifficultyButton.onClick.AddListener(SetHardDifficulty);

        saveButton.onClick.AddListener(SaveSetting);
 
    }

    public void LoadSetting()
    {
        if(PlayerPrefs.HasKey("TimeBetweenWaves"))
        {
            timeBetweenWaves = PlayerPrefs.GetFloat("TimeBetweenWaves");
        }
        if (PlayerPrefs.HasKey("ZombiePerWave"))
        {
            timeBetweenWaves = PlayerPrefs.GetInt("ZombiePerWave");
        }


    }

    public void SaveSetting()
    {
        PlayerPrefs.SetFloat("TimeBetweenWaves", timeBetweenWaves);
        PlayerPrefs.SetInt("ZombiePerWave", zombiePerWave);

        PlayerPrefs.Save();
    }

    private void SetNormalDifficulty()
    {
        timeBetweenWaves = 10f;
        zombiePerWave = 4;
    }

    private void SetHardDifficulty()
    {
        timeBetweenWaves = 7f;
        zombiePerWave = 8;
    }

    
}
