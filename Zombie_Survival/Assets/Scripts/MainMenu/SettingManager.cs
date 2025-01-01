using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Button map1Button;

    void Start()
    {
        if (map1Button != null)
        {
            map1Button.onClick.AddListener(() => LoadScene("Map1"));
        }
        else
        {
            Debug.LogError("Map1 Button is not assigned in the Inspector!");
        }
    }

    // Phương thức chung để load Scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}