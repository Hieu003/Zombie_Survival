using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HQFPSWeapons;


public class DeathScreen : MonoBehaviour
{
    public Image targetImage;
    public Text targetText;

    public float duration = 5f;

    public bool showDeadScreen = false;

    private float targetAplha = 1f;

    private float startAplha;

    private float elapsedTime = 0f;

    public PlayerDeath playerDeath;

    private void Start()
    {
        startAplha = targetImage.color.a;
        if (playerDeath != null)
        {
            playerDeath.OnPlayerDeath += ShowDeathScreen;
        }
        else
        {
            Debug.LogError("PlayerDeath reference is not set in DeathScreen!");
        }
        // Ẩn màn hình chết khi bắt đầu game
        HideUI();
    }


    private void OnDestroy()
    {
        // Hủy đăng ký lắng nghe sự kiện khi DeathScreen bị hủy
        if (playerDeath != null)
        {
            playerDeath.OnPlayerDeath -= ShowDeathScreen;
        }
    }

    public void ShowDeathScreen()
    {
        showDeadScreen = true;
    }
    public void HideUI()
    {
        showDeadScreen = false;
        Color newColor = targetImage.color;
        newColor.a = 0f;
        targetImage.color = newColor;

        Color newTextAplha = targetText.color;
        newTextAplha.a = 0f;
        targetText.color = newTextAplha;
        elapsedTime = 0f;
        Time.timeScale = 1f;
    }
    private void Update()
    {
        if(showDeadScreen)
        {
            if(elapsedTime < duration)
            {
                float newAplha = Mathf.Lerp(startAplha, targetAplha, elapsedTime / duration);

                Color newColor = targetImage.color;
                newColor.a = newAplha;
                targetImage.color = newColor;

                Color newTextAplha = targetText.color;
                newTextAplha.a = newAplha;
                targetText.color = newTextAplha;

                elapsedTime += Time.deltaTime;
            }
            else
            {
                Time.timeScale = 0f;
            }
        }
    }

}
