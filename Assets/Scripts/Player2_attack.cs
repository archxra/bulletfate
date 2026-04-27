using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon_PlasmaGun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float cooldown = 0.4f; // кд из документа
    private float nextFireTime;

    void Update()
    {
        // Стреляем на ЛКМ
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            ShootPlasma();
            nextFireTime = Time.time + cooldown;
        }
    }

    void ShootPlasma()
    {
        // Выпускаем 3 пули: одну прямо, одну на -30 градусов, одну на +30 градусов.
        // Итого разброс 60 градусов (это и будет наш сектор атаки)
        float[] angles = { -30f, 0f, 30f };

        foreach (float a in angles)
        {
            // Берем текущий поворот игрока и добавляем к нему угол пули
            Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, a);
            Instantiate(bulletPrefab, firePoint.position, rotation);
        }
    }
}