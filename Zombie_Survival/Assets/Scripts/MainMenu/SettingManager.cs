using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Button map1Button;
    public Button map2Button;
    public Button easyButton; // Nút Easy
    public Button hardButton;

    void Start()
    {
        if (map1Button != null)
        {
            map1Button.onClick.AddListener(() => LoadScene("Map1"));
        }

        if (map2Button != null)
        {
            map2Button.onClick.AddListener(() => LoadScene("Map2"));
        }

        if (easyButton != null)
        {
            easyButton.onClick.AddListener(SetEasyDifficulty);
        }

        if (hardButton != null)
        {
            hardButton.onClick.AddListener(SetHardDifficulty);
        }

        
    }

    // Phương thức chung để load Scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SetEasyDifficulty()
    {
        PlayerPrefs.SetString("Difficulty", "Easy");
        PlayerPrefs.Save();
        Debug.Log("Difficulty set to Easy");
    }

    // Phương thức khi chọn Hard
    public void SetHardDifficulty()
    {
        PlayerPrefs.SetString("Difficulty", "Hard");
        PlayerPrefs.Save();
        Debug.Log("Difficulty set to Hard");
    }
}