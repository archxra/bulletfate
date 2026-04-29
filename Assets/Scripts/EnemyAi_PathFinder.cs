using UnityEngine;

public class Enemy_Smart : MonoBehaviour
{
    public float speed = 3f;
    public float attackDistance = 1.5f; // Дистанция, где бьет
    public Animator anim;
    public float attackCooldown = 2f;

    private Rigidbody2D rb;
    private Transform player;
    private float nextAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // 1. Ищем игрока
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) { rb.linearVelocity = Vector2.zero; return; }
        player = p.transform;

        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 dir = (player.position - transform.position).normalized;

        // 2. Логика боя
        if (dist <= attackDistance)
        {
            // МЫ В ЗОНЕ УДАРА
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isMoving", false);

            if (Time.time >= nextAttackTime)
            {
                TriggerAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            // ИДЕМ К ИГРОКУ
            rb.linearVelocity = dir * speed;

            // Анимация ходьбы
            anim.SetBool("isMoving", true);

            // Поворот (Flip)
            if (dir.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(dir.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void TriggerAttack()
    {
        // Включаем триггер атаки
        anim.SetTrigger("isAttacking");

        // Наносим урон (можно добавить задержку через корутину, если анимация долгая)
        player.GetComponent<Health>()?.TakeDamage(2f);
        Debug.Log("Враг атаковал!");
    }
}