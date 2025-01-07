using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQFPSWeapons;

public class SoundRelay : MonoBehaviour
{

    public float hearingRadius = 10000f; // Bán kính nghe thấy âm thanh
    private Player player;
    private Gun gun;

    private void Start()
    {
        // Lấy component Player
        player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("No Player component found on this GameObject!");
        }
    }

    public void RegisterGunEvents()
    {
        if (player != null)
        {
            gun = player.GetComponentInChildren<Gun>();
            if (gun != null)
            {
                gun.OnShoot += OnGunShoot;
            }
            else
            {
                Debug.LogWarning("No Gun component found in children of Player!");
            }
        }
    }

    private void OnGunShoot(Vector3 position, float loudness)
    {
        // Lấy tất cả các collider trong bán kính nghe thấy
        Collider[] colliders = Physics.OverlapSphere(position, hearingRadius);

        foreach (Collider col in colliders)
        {
            // Kiểm tra xem collider có phải là zombie không
            BaseZombieAI zombie = col.GetComponent<BaseZombieAI>();
            if (zombie != null)
            {
                // Thông báo cho zombie về âm thanh
                zombie.HearSound(position, loudness);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Hiển thị bán kính nghe thấy trong scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}
