using UnityEngine;
using UnityEngine.InputSystem; // Обязательно добавляем эту строку для новой системы!

public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Проверяем, есть ли вообще клавиатура и мышь
        if (Keyboard.current == null || Mouse.current == null) return;

        // Считываем WASD вручную
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.dKey.isPressed) moveX += 1f;
        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.wKey.isPressed) moveY += 1f;
        if (Keyboard.current.sKey.isPressed) moveY -= 1f;

        moveInput = new Vector2(moveX, moveY);

        // Получаем позицию мыши на экране и переводим в координаты игры
        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        mousePosition = Camera.main.ScreenToWorldPoint(screenMousePos);
    }

    void FixedUpdate()
    {
        // Двигаем
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);

        // Поворачиваем кубик (квадрат) в сторону мышки
        Vector2 lookDirection = mousePosition - rb.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }
}