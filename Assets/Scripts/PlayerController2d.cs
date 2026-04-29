using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;
    public SpriteRenderer bodyRenderer; // Спрайт героя

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ищем компоненты строго на самом игроке
        animator = GetComponent<Animator>();
        bodyRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // Чтение кнопок WASD
        float moveX = 0;
        float moveY = 0;
        if (Keyboard.current.dKey.isPressed) moveX = 1;
        else if (Keyboard.current.aKey.isPressed) moveX = -1;

        if (Keyboard.current.wKey.isPressed) moveY = 1;
        else if (Keyboard.current.sKey.isPressed) moveY = -1;

        moveInput = new Vector2(moveX, moveY);

        // Передаем в аниматор
        if (animator != null)
        {
            animator.SetBool("isMoving", moveInput.magnitude > 0.1f);
        }
    }

    void FixedUpdate()
    {
        // Движение физики
        rb.linearVelocity = moveInput.normalized * moveSpeed;

        if (Mouse.current != null && Camera.main != null && bodyRenderer != null)
        {
            // Получаем позицию мышки
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            // ПОВОРОТ ТОЛЬКО КАРТИНКИ (чтобы не сломать ружье)
            if (mousePos.x < transform.position.x)
                bodyRenderer.flipX = true;
            else
                bodyRenderer.flipX = false;
        }
    }
}