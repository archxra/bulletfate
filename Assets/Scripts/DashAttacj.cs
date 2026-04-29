using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Skill_DashAttack : MonoBehaviour
{
    [Header("Настройки рывка")]
    public float dashForce = 12f;
    public float dashDuration = 0.3f;
    public float cooldown = 15f;

    [Header("Визуал и Анимация")]
    public Animator playerAnimator; // Аниматор игрока
    public GameObject weaponObject; // Объект ружья (weapon)

    [Header("Настройки атаки (Меч)")]
    public float damage = 5f;
    public Transform attackPoint;
    public Vector2 attackSize = new Vector2(3f, 2f);
    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private PlayerController2D playerController;
    private bool isDashing = false;
    private float nextUseTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController2D>();
    }

    void Update()
    {
        if (Mouse.current == null || isDashing) return;

        if (Mouse.current.rightButton.wasPressedThisFrame && Time.time >= nextUseTime)
        {
            StartCoroutine(PerformDashAttack());
            nextUseTime = Time.time + cooldown;
        }
    }

    IEnumerator PerformDashAttack()
    {
        isDashing = true;
        if (playerController != null) playerController.enabled = false;

        // 1. ВКЛЮЧАЕМ АНИМАЦИЮ И ПРЯЧЕМ РУЖЬЕ
        if (playerAnimator != null) playerAnimator.SetTrigger("Skill");
        if (weaponObject != null) weaponObject.SetActive(false);

        // 2. Логика рывка
        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 10f));
        Vector2 dashDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        Health playerHealth = GetComponent<Health>();
        if (playerHealth != null) playerHealth.BecomeInvincible(dashDuration + 0.3f);

        rb.linearVelocity = dashDirection * dashForce;
        AttackEnemiesInRectangle(dashDirection);

        yield return new WaitForSeconds(dashDuration);

        // 3. ВОЗВРАЩАЕМ РУЖЬЕ
        rb.linearVelocity = Vector2.zero;
        if (weaponObject != null) weaponObject.SetActive(true);

        if (playerController != null) playerController.enabled = true;
        isDashing = false;
    }

    void AttackEnemiesInRectangle(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Vector3 zoneCenter = transform.position + (Vector3)direction * 1.5f;

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(zoneCenter, attackSize, angle, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Health>()?.TakeDamage(damage);
        }
    }
}