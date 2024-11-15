using UnityEngine;

public class CartridgeObject : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Bullet = null;

    [SerializeField]
    private GameObject m_NewCartridgeMesh = null;

    private Renderer m_Renderer;


    public void ChangeBulletState(bool activate) 
    {
        if(m_Bullet != null)
            m_Bullet.SetActive(activate);
    }

    public void ChangeCartridgeState(bool activate) 
    {
        gameObject.SetActive(activate);
    }

    public void ChangeCartridgeMesh(bool changeToOriginal)
    {
        if (changeToOriginal)
        {
            m_NewCartridgeMesh.SetActive(false);
            m_Renderer.enabled = true; 
        }
        else
        {
            m_NewCartridgeMesh.SetActive(true);
            m_Renderer.enabled = false;
        }
    }

    private void Start()
    {
        if (m_NewCartridgeMesh != null)
        {
            m_NewCartridgeMesh.SetActive(false);
            m_Renderer = GetComponent<Renderer>();
        }
    }
}
