using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    private Transform target;       // За кем следим
    public float smoothSpeed = 0.125f; // Скорость сглаживания (чем меньше, тем медленнее камера)
    public Vector3 offset = new Vector3(0, 0, -10); // Смещение камеры относительно игрока

    void FixedUpdate()
    {
        // Постоянно ищем игрока (так как ты переключаешь персонажей 1-2-3-4-5)
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        if (target == null) return;

        // Рассчитываем позицию, в которой должна быть камера
        Vector3 desiredPosition = target.position + offset;

        // Плавно перемещаем камеру из текущей точки в нужную
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}