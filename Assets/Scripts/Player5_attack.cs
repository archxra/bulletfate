using UnityEngine;
using UnityEngine.InputSystem;

public class Soldier_Thompson : MonoBehaviour
{
    public GameObject bulletPrefab; // Маленькая желтая пуля (Damage: 1)
    public Transform firePoint;
    public float cooldown = 0.1f; // Огромная скорострельность!
    private float nextFire;

    void Update()
    {
        // ЛКМ - зажимаем и строчим
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFire)
        {
            Instantiate(bulletPrefab, firePoint.position, transform.rotation);
            nextFire = Time.time + cooldown;
        }
    }
}