using UnityEngine;
using UnityEngine.InputSystem; // Для считывания мышки

public class SidekickLogic : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float shootDelay = 0.2f;

    void Start()
    {
        // Небольшая пауза, чтобы игрок успел увидеть дронов перед выстрелом
        Invoke("ShootAtMouseAndDestroy", shootDelay);
    }

    void ShootAtMouseAndDestroy()
    {
        if (Mouse.current == null) return;

        // 1. Получаем позицию мышки в мировых координатах
        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 10f));

        // 2. Считаем направление от САМОГО ДРОНА до МЫШКИ
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)transform.position;
        direction.Normalize();

        // 3. Считаем угол поворота (чтобы пуля летела "носом" вперед)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

        // 4. Создаем пулю
        Instantiate(bulletPrefab, transform.position, bulletRotation);

        // Дрон исчезает сразу после выстрела
        Destroy(gameObject, 0.1f);
    }
}