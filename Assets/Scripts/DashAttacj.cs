using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Skill_DashAttack : MonoBehaviour
{
    [Header("Настройки рывка")]
    public float dashForce = 20f; // Сделал силу чуть больше для наглядности
    public float dashDuration = 0.2f;
    public float cooldown = 15f;

    [Header("Настройки атаки (Меч)")]
    public float damage = 5f;
    public Transform attackPoint;
    public Vector2 attackSize = new Vector2(3f, 2f);
    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private PlayerController2D playerController; // Ссылка на скрипт ходьбы
    private bool isDashing = false;
    private float nextUseTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController2D>(); // Находим скрипт ходьбы на игроке
    }

    void Update()
    {
        if (Keyboard.current == null || isDashing) return;

        // Нажали Пробел
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (Time.time >= nextUseTime)
            {
                Debug.Log("Пробел нажат! Делаем рывок!");
                StartCoroutine(PerformDashAttack());
                nextUseTime = Time.time + cooldown;
            }
            else
            {
                // Показывает сколько секунд осталось до отката
                Debug.Log("Рывок на кулдауне! Осталось: " + (nextUseTime - Time.time).ToString("F1") + " сек");
            }
        }
    }

    IEnumerator PerformDashAttack()
    {
        isDashing = true;

        // 1. Отключаем обычную ходьбу, чтобы она не мешала рывку
        if (playerController != null) playerController.enabled = false;

        Health playerHealth = GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.BecomeInvincible(dashDuration + 0.3f);
        }
        // 2. Толкаем игрока вперед
        Vector2 dashDirection = transform.up;
        rb.linearVelocity = dashDirection * dashForce;

        // 3. Бьем врагов
        AttackEnemiesInRectangle();

        // 4. Ждем доли секунды, пока он летит
        yield return new WaitForSeconds(dashDuration);

        // 5. Останавливаем рывок и Включаем ходьбу обратно
        rb.linearVelocity = Vector2.zero;
        if (playerController != null) playerController.enabled = true;

        isDashing = false;
    }

    void AttackEnemiesInRectangle()
    {
        if (attackPoint == null)
        {
            Debug.LogError("ОШИБКА: Не назначена Attack Point в инспекторе!");
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            attackPoint.position,
            attackSize,
            transform.eulerAngles.z,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log("Попал рывком по: " + enemy.name);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(attackPoint.position, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(attackSize.x, attackSize.y, 0));
    }
}