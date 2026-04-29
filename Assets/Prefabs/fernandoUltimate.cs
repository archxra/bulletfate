using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FernandoUltraController : MonoBehaviour
{
    public GameObject bulletPrefab; // Твой префаб пули
    public Transform[] firePoints;  // Перетащи сюда 4 точки из иерархии
    public float waitTime = 1f;     // Сколько ждем до выстрела

    void Start()
    {
        // 1. Запускаем таймер выстрела
        StartCoroutine(ShootAfterDelay());
    }

    IEnumerator ShootAfterDelay()
    {
        // Ждем время по таймеру
        yield return new WaitForSeconds(waitTime);

        if (Mouse.current != null)
        {
            // Берем позицию мышки
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 10f));

            // Стреляем из каждого FirePoint
            foreach (Transform fp in firePoints)
            {
                if (fp == null) continue;

                Vector2 direction = (Vector2)mouseWorldPos - (Vector2)fp.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

                Instantiate(bulletPrefab, fp.position, Quaternion.Euler(0, 0, angle));
            }
        }

        // Удаляем весь объект ульты (вместе с дронами) через 0.1 сек после выстрела
        Destroy(gameObject, 0.1f);
    }
}