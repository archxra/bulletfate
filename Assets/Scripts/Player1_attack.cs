using UnityEngine;
using UnityEngine.InputSystem; // Подключаем новую систему

public class PlayerAttack2D : MonoBehaviour
{
    [Header("Настройки оружия")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float attackCooldown = 0.8f; // Кулдаун 0.8 сек из документа

    private float nextFireTime = 0f;

    void Update()
    {
        if (Mouse.current == null) return;

        // Если нажата Левая Кнопка Мыши и прошел кулдаун
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + attackCooldown;
        }
    }

    void Shoot()
    {
        // Создаем пулю
        Instantiate(bulletPrefab, firePoint.position, transform.rotation);
    }
}