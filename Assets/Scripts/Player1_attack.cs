using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack2D : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint; // Пустышка на конце ствола ружья
    public float attackCooldown = 0.8f;

    [Tooltip("Аниматор ружья (объект WeaponSprite)")]
    public Animator weaponAnimator;

    private float nextFireTime = 0f;

    void Update()
    {
        if (Mouse.current == null || Camera.main == null) return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + attackCooldown;
        }
    }

    void Shoot()
    {
        if (firePoint == null || bulletPrefab == null) return;

        // В 2D мы просто берем поворот самого ружья (firePoint крутится вместе с ним)
        // Если пуля летит "боком", нужно добавить смещение угла (например, +90 или -90)
        // В нашем случае, если спрайт пули смотрит вверх, используем этот код:

        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 10f));
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)firePoint.position;

        // Считаем угол и вычитаем 90 градусов (стандарт для 2D-спрайтов, смотрящих вверх)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));

        // Вызываем анимацию выстрела
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Fire");
        }
    }
}