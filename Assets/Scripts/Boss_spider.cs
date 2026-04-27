using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SpiderQueen_Complete : MonoBehaviour
{
    [Header("Умная Дистанция")]
    public float speed = 3.5f;
    public float retreatSpeed = 4.5f;   // Скорость, когда пятится от игрока
    public float shootDistance = 7f;    // С какого расстояния начинает стрелять
    public float meleeDistance = 3f;    // Если игрок ближе - бьет по земле
    public LayerMask obstacleLayer;     // Слой стен (Obstacle)

    [Header("Рендж Атака (Паутина)")]
    public GameObject webPrefab;
    public Transform firePoint;
    public float bulletSpeed = 12f;
    public int burstCount = 4;
    public float burstInterval = 0.8f;
    public float rangedCooldown = 4f;

    [Header("Мили Атака (Удар по земле)")]
    public float meleeDamage = 3f;
    public float meleeCooldown = 2f;

    // Системные переменные
    private Transform player;
    private Rigidbody2D playerRb;
    private Rigidbody2D rb;
    private List<Vector3> path;
    private int waypointIndex = 0;

    // Состояния (State Machine)
    private bool isAttacking = false;
    private float nextRangedTime = 0f;
    private float nextMeleeTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Запускаем мозги (Поиск пути и Игрока)
        InvokeRepeating("ThinkAndPathfind", 0f, 0.4f);
    }

    // 1. МОЗГ БОССА (Куда идти?)
    void ThinkAndPathfind()
    {
        // ПОСТОЯННО ИЩЕМ ИГРОКА (Вдруг он переключился)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerRb = p.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogWarning("БОСС: Ищу игрока...");
            return; // Ждем, пока игрок не появится
        }

        if (isAttacking || AStar2D.Instance == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D wallHit = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
        bool canSeePlayer = (wallHit.collider == null);

        Vector3 targetPos = transform.position; // По умолчанию стоим

        if (!canSeePlayer || dist > shootDistance)
        {
            // Далеко или за стеной -> Идем к игроку
            targetPos = player.position;
        }
        else if (dist < meleeDistance && Time.time >= nextMeleeTime)
        {
            // Игрок в упор -> Не строим путь, сейчас будем бить
            path = null;
            return;
        }
        else if (dist < meleeDistance && Time.time < nextMeleeTime)
        {
            // Игрок в упор, но атака на КД -> Пятимся назад!
            Vector2 dirAway = (transform.position - player.position).normalized;
            targetPos = transform.position + (Vector3)dirAway * 4f;
        }
        else
        {
            // Идеальная дистанция (между melee и shoot) -> Стоим и стреляем
            path = null;
            return;
        }

        // Строим путь через A*
        path = AStar2D.Instance.FindPath(transform.position, targetPos);
        waypointIndex = 0;
    }

    // 2. ДЕЙСТВИЯ (Атаки)
    void Update()
    {
        // Проверка на Стан (если Гладиатор крикнул)
        if (!enabled || player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D wallHit = Physics2D.Linecast(transform.position, player.position, obstacleLayer);

        // Приоритет №1: Игрок в упор -> Бьем по земле
        if (dist <= meleeDistance && Time.time >= nextMeleeTime)
        {
            StartCoroutine(MeleeSlamAttack());
        }
        // Приоритет №2: Игрок на мушке -> Стреляем с упреждением
        else if (wallHit.collider == null && dist <= shootDistance && Time.time >= nextRangedTime)
        {
            StartCoroutine(RangedBurstAttack());
        }
    }

    // 3. ФИЗИКА (Движение)
    void FixedUpdate()
    {
        if (!enabled || isAttacking)
        {
            rb.linearVelocity = Vector2.zero; // Во время атаки или стана стоим как вкопанные
            return;
        }

        if (path != null && waypointIndex < path.Count)
        {
            Vector3 target = path[waypointIndex];
            Vector2 dir = (target - transform.position).normalized;

            float dist = Vector2.Distance(transform.position, player.position);
            float currentSpeed = (dist < meleeDistance) ? retreatSpeed : speed; // Ускоряемся, когда пятимся

            rb.MovePosition(rb.position + dir * currentSpeed * Time.fixedDeltaTime);

            if (Vector2.Distance(transform.position, target) < 0.2f) waypointIndex++;
        }
    }

    // --- КОРУТИНЫ АТАК ---

    IEnumerator RangedBurstAttack()
    {
        isAttacking = true;
        path = null; // Сбрасываем путь
        rb.linearVelocity = Vector2.zero;

        // Даем боссу полсекунды "прицелиться" перед выстрелом
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < burstCount; i++)
        {
            if (player == null) break;
            ShootWithPrediction();
            yield return new WaitForSeconds(burstInterval);
        }

        nextRangedTime = Time.time + rangedCooldown;
        isAttacking = false;
    }

    void ShootWithPrediction()
    {
        Vector2 pVel = (playerRb != null) ? playerRb.linearVelocity : Vector2.zero;
        float dist = Vector2.Distance(transform.position, player.position);
        float timeToHit = dist / bulletSpeed;

        // ПРЕДСКАЗАНИЕ: Точка = Позиция игрока + (его скорость * время полета пули)
        Vector2 predictedPos = (Vector2)player.position + (pVel * timeToHit);
        Vector2 dir = (predictedPos - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Transform fp = (firePoint != null) ? firePoint : transform;
        Instantiate(webPrefab, fp.position, Quaternion.Euler(0, 0, angle));
    }

    IEnumerator MeleeSlamAttack()
    {
        isAttacking = true;
        path = null;
        rb.linearVelocity = Vector2.zero;

        // Визуальный эффект замаха (босс надувается)
        Vector3 origScale = transform.localScale;
        transform.localScale = origScale * 1.3f;

        // Время на реакцию игроку (0.4 сек)
        yield return new WaitForSeconds(0.4f);

        // УРОН (Проверяем, успел ли игрок убежать)
        if (Vector2.Distance(transform.position, player.position) <= meleeDistance + 0.5f)
        {
            player.GetComponent<Health>()?.TakeDamage(meleeDamage);
            Debug.Log("БОСС: Нанес мили-урон!");
        }

        // Возвращаемся в норму
        transform.localScale = origScale;

        nextMeleeTime = Time.time + meleeCooldown;

        // Небольшая задержка перед тем, как босс снова начнет двигаться
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // Рисуем зоны в редакторе для удобства настройки
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootDistance);
    }
}