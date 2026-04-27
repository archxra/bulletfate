using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Eel_Complete : MonoBehaviour
{
    [Header("=== ДВИЖЕНИЕ И ИИ ===")]
    [Tooltip("Обычная скорость преследования")]
    public float speed = 4f;
    [Tooltip("Скорость, когда босс пятится от игрока")]
    public float retreatSpeed = 5f;
    [Tooltip("Как быстро босс поворачивает 'морду' (чем больше, тем резче)")]
    public float rotationSpeed = 10f;
    [Tooltip("Как часто босс обновляет маршрут A* (в секундах)")]
    public float aiUpdateInterval = 0.4f;
    [Tooltip("На сколько метров назад отплывает босс, если игрок слишком близко")]
    public float retreatDistanceOffset = 4f;
    [Tooltip("Слой стен, сквозь которые босс не видит")]
    public LayerMask obstacleLayer;

    [Header("=== ДИСТАНЦИИ (Зоны) ===")]
    [Tooltip("С какого расстояния босс начинает стрелять (Желтый круг)")]
    public float shootDistance = 6f;
    [Tooltip("Радиус удара хвостом и дистанция испуга (Красный круг)")]
    public float meleeDistance = 3.5f;

    [Header("=== АТАКА 1: Шаровые молнии ===")]
    public GameObject ballPrefab;
    public Transform firePoint;
    [Tooltip("Перезарядка обычных выстрелов")]
    public float ballCooldown = 1.5f;

    [Header("=== АТАКА 2: Цепная молния (Барьер) ===")]
    public GameObject chainPrefab;
    [Tooltip("Перезарядка барьера")]
    public float chainCooldown = 6f;
    [Tooltip("Сколько секунд босс 'надувается' перед выстрелом")]
    public float chainCastTime = 0.6f;
    [Tooltip("Сколько секунд босс стоит на месте после выстрела")]
    public float chainPostPause = 0.4f;
    [Tooltip("Во сколько раз босс увеличивается при касте")]
    public float chainSwellScale = 1.2f;

    [Header("=== АТАКА 3: Удар хвостом ===")]
    [Tooltip("Урон от удара хвостом")]
    public float tailDamage = 5f;
    [Tooltip("Перезарядка удара хвостом")]
    public float tailCooldown = 3f;
    [Tooltip("Сколько секунд босс замахивается (время на реакцию игроку)")]
    public float tailCastTime = 0.4f;
    [Tooltip("Сколько секунд босс отдыхает после удара")]
    public float tailPostPause = 0.5f;
    [Tooltip("Как сильно босс сплющивается по оси X во время удара")]
    public float tailScaleX = 0.8f;
    [Tooltip("Как сильно босс вытягивается по оси Y во время удара")]
    public float tailScaleY = 1.5f;
    [Tooltip("Объект (красный полупрозрачный круг), который показывает зону удара")]
    public GameObject tailWarningVisual;

    // Системные переменные (скрыты от дизайнера)
    private Transform player;
    private Rigidbody2D rb;
    private List<Vector3> path;
    private int waypointIndex = 0;

    // Состояния
    private bool isAttacking = false;
    private float nextBallTime = 0f;
    private float nextChainTime = 0f;
    private float nextTailTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (tailWarningVisual != null) tailWarningVisual.SetActive(false);

        // Мозг думает по настраиваемому таймеру
        InvokeRepeating("ThinkAndPathfind", 0f, aiUpdateInterval);
    }

    void ThinkAndPathfind()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        else return;

        if (isAttacking || AStar2D.Instance == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D wallHit = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
        bool canSeePlayer = (wallHit.collider == null);

        Vector3 targetPos = transform.position;

        if (!canSeePlayer || dist > shootDistance)
        {
            // Игрок далеко или за стеной -> Плывем к нему
            targetPos = player.position;
        }
        else if (dist < meleeDistance && Time.time < nextTailTime)
        {
            // Игрок слишком близко, но хвост на перезарядке -> Отплываем назад
            Vector2 dirAway = (transform.position - player.position).normalized;
            targetPos = transform.position + (Vector3)dirAway * retreatDistanceOffset;
        }
        else
        {
            // Идеальная дистанция для стрельбы -> Стоим на месте
            path = null;
            return;
        }

        path = AStar2D.Instance.FindPath(transform.position, targetPos);
        waypointIndex = 0;
    }

    void Update()
    {
        if (!enabled || player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D wallHit = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
        bool canSeePlayer = (wallHit.collider == null);

        // Приоритет 1: Хвост
        if (dist <= meleeDistance && Time.time >= nextTailTime)
        {
            StartCoroutine(TailSmashAttack());
        }
        // Приоритет 2: Цепная молния
        else if (canSeePlayer && dist <= shootDistance && Time.time >= nextChainTime)
        {
            StartCoroutine(ChainLightningAttack());
        }
        // Приоритет 3: Обычные шары
        else if (canSeePlayer && dist <= shootDistance && Time.time >= nextBallTime)
        {
            ShootBall();
            nextBallTime = Time.time + ballCooldown;
        }
    }

    void FixedUpdate()
    {
        if (!enabled || isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (path != null && waypointIndex < path.Count)
        {
            Vector3 target = path[waypointIndex];
            Vector2 dir = (target - transform.position).normalized;

            float dist = Vector2.Distance(transform.position, player.position);
            float currentSpeed = (dist < meleeDistance) ? retreatSpeed : speed;

            rb.MovePosition(rb.position + dir * currentSpeed * Time.fixedDeltaTime);

            // Плавный поворот
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), rotationSpeed * Time.fixedDeltaTime);
            }

            if (Vector2.Distance(transform.position, target) < 0.2f) waypointIndex++;
        }
    }

    void ShootBall()
    {
        if (ballPrefab == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Transform fp = (firePoint != null) ? firePoint : transform;
        Instantiate(ballPrefab, fp.position, Quaternion.Euler(0, 0, angle));
    }

    IEnumerator ChainLightningAttack()
    {
        isAttacking = true;
        path = null;
        rb.linearVelocity = Vector2.zero;

        Vector3 origScale = transform.localScale;
        transform.localScale = origScale * chainSwellScale;

        yield return new WaitForSeconds(chainCastTime);

        if (player != null && chainPrefab != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            Transform fp = (firePoint != null) ? firePoint : transform;
            Instantiate(chainPrefab, fp.position, Quaternion.Euler(0, 0, angle));
        }

        transform.localScale = origScale;
        nextChainTime = Time.time + chainCooldown;

        yield return new WaitForSeconds(chainPostPause);
        isAttacking = false;
    }

    IEnumerator TailSmashAttack()
    {
        isAttacking = true;
        path = null;
        rb.linearVelocity = Vector2.zero;

        if (tailWarningVisual != null) tailWarningVisual.SetActive(true);

        Vector3 origScale = transform.localScale;
        transform.localScale = new Vector3(origScale.x * tailScaleX, origScale.y * tailScaleY, origScale.z);

        yield return new WaitForSeconds(tailCastTime);

        if (player != null && Vector2.Distance(transform.position, player.position) <= meleeDistance)
        {
            player.GetComponent<Health>()?.TakeDamage(tailDamage);
        }

        if (tailWarningVisual != null) tailWarningVisual.SetActive(false);
        transform.localScale = origScale;

        nextTailTime = Time.time + tailCooldown;
        yield return new WaitForSeconds(tailPostPause);
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootDistance);
    }
}