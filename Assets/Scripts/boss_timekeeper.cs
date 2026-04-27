using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_TimeKeeper_Complete : MonoBehaviour
{
    [Header("=== АРЕНА (Зона Телепорта) ===")]
    [Tooltip("Центр комнаты босса (где он спавнится)")]
    public Transform arenaCenter;
    [Tooltip("Размер комнаты (Ширина и Высота)")]
    public Vector2 arenaSize = new Vector2(20f, 15f);
    public LayerMask obstacleLayer; // Слой стен

    [Header("=== СТАТЫ БОССА ===")]
    public int currentPhase = 1;
    [Tooltip("Как часто босс прыгает (в секундах)")]
    public float teleportCooldown = 3f;
    [Tooltip("Как часто босс атакует (в секундах)")]
    public float attackCooldown = 2f;

    [Header("=== ФАЗА 1: Желтые Стрелы ===")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    [Tooltip("Сколько стрел выпускает за раз")]
    public int phase1ArrowCount = 3;
    [Tooltip("Угол между стрелами (веер)")]
    public float phase1SpreadAngle = 15f;
    [Tooltip("Пауза между выстрелами стрел")]
    public float phase1BurstDelay = 0.3f;

    [Header("=== ФАЗА 2: Сферы Времени (Стан) ===")]
    public GameObject sphereVisualPrefab; // Синий шарик (Trigger)
    [Tooltip("Скорость полета синей сферы")]
    public float sphereSpeed = 5f;
    [Tooltip("На сколько секунд сфера станит игрока")]
    public float sphereStunTime = 2f;
    [Tooltip("Пауза перед выстрелом стрелы после сфер")]
    public float phase2ArrowDelay = 0.5f;

    [Header("=== ФАЗА 3: Град Света (Ульта) ===")]
    public GameObject lightRainVisualPrefab; // Огромный круг
    [Tooltip("Кулдаун ульты")]
    public float ultCooldown = 15f;
    [Tooltip("Сколько волн (взрывов) падает за одну ульту")]
    public int ultWaveCount = 3;
    [Tooltip("Сколько кругов падает в одной волне")]
    public int ultCirclesPerWave = 4;
    [Tooltip("Радиус разброса кругов вокруг игрока (чем больше, тем шире арена взрывов)")]
    public float ultSpreadRadius = 7f;
    [Tooltip("Задержка перед взрывом круга (время на побег)")]
    public float ultExplosionDelay = 1.2f;
    [Tooltip("Радиус урона одного круга")]
    public float ultExplosionRadius = 3.5f;
    [Tooltip("Урон от одного круга")]
    public float ultDamage = 15f;

    // Системные переменные
    private Transform player;
    private Health hpScript;
    private float maxHp;
    private bool isAttacking = false;
    private float nextTpTime;
    private float nextAttackTime;
    private float nextUltTime;

    void Start()
    {
        hpScript = GetComponent<Health>();
        maxHp = hpScript.maxHealth;

        // Если центр арены не задан, считаем центр арены там, где босс появился
        if (arenaCenter == null) arenaCenter = this.transform;

        nextTpTime = Time.time + teleportCooldown;
        nextAttackTime = Time.time + attackCooldown;
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (isAttacking) return;

        CheckPhase();

        // 1. ТЕЛЕПОРТАЦИЯ (Только внутри Арены)
        if (Time.time >= nextTpTime)
        {
            TeleportInsideArena();
            nextTpTime = Time.time + teleportCooldown;
        }

        // 2. УЛЬТА (Наивысший приоритет в 3 фазе)
        if (currentPhase == 3 && Time.time >= nextUltTime)
        {
            StartCoroutine(Phase3_UltimateAttack());
            return;
        }

        // 3. ОБЫЧНЫЕ АТАКИ
        if (Time.time >= nextAttackTime)
        {
            if (currentPhase == 1) StartCoroutine(Phase1_ArrowAttack());
            else if (currentPhase == 2) StartCoroutine(Phase2_SphereAttack());
            else if (currentPhase == 3) StartCoroutine(Phase2_SphereAttack());

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void CheckPhase()
    {
        float hpPercent = hpScript.currentHealth / maxHp;

        if (hpPercent <= 0.33f && currentPhase != 3)
        {
            currentPhase = 3;
            Debug.Log("<color=red>ФАЗА 3: АБСОЛЮТНАЯ ВЛАСТЬ!</color>");
            teleportCooldown /= 2f;
            attackCooldown /= 2f;
            nextUltTime = Time.time + 1f; // Сразу дает ульту
        }
        else if (hpPercent <= 0.66f && currentPhase == 1)
        {
            currentPhase = 2;
            Debug.Log("<color=yellow>ФАЗА 2: ВРЕМЯ ОСТАНОВЛЕНО!</color>");
        }
    }

    // --- ТЕЛЕПОРТ В ПРЕДЕЛАХ КОМНАТЫ ---
    void TeleportInsideArena()
    {
        // Выбираем случайную точку строго внутри прямоугольника Арены
        float randomX = Random.Range(-arenaSize.x / 2f, arenaSize.x / 2f);
        float randomY = Random.Range(-arenaSize.y / 2f, arenaSize.y / 2f);
        Vector3 tpTarget = arenaCenter.position + new Vector3(randomX, randomY, 0);

        // Проверяем, не попали ли мы в стену внутри арены
        if (Physics2D.OverlapCircle(tpTarget, 1f, obstacleLayer) == null)
        {
            StartCoroutine(TeleportVisuals(tpTarget));
        }
    }

    IEnumerator TeleportVisuals(Vector3 targetPos)
    {
        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.2f);
        transform.position = targetPos;
        GetComponent<SpriteRenderer>().enabled = true;
    }

    // --- АТАКИ ---

    IEnumerator Phase1_ArrowAttack()
    {
        isAttacking = true;
        if (arrowPrefab == null) { isAttacking = false; yield break; }

        Vector2 dir = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // Вычисляем веер стрел (справа налево)
        float startAngle = baseAngle + (phase1SpreadAngle * (phase1ArrowCount - 1) / 2f);

        for (int i = 0; i < phase1ArrowCount; i++)
        {
            float currentAngle = startAngle - (phase1SpreadAngle * i);
            Instantiate(arrowPrefab, firePoint.position, Quaternion.Euler(0, 0, currentAngle));
            yield return new WaitForSeconds(phase1BurstDelay);
        }
        isAttacking = false;
    }

    IEnumerator Phase2_SphereAttack()
    {
        isAttacking = true;
        if (sphereVisualPrefab == null) { isAttacking = false; yield break; }

        // 8 сфер крестом со смещением (чтобы было "окно" для уклонения)
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            SpawnTimeSphere(angle - 15f);
            SpawnTimeSphere(angle + 15f);
        }

        yield return new WaitForSeconds(phase2ArrowDelay);

        if (arrowPrefab != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            float aimAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            Instantiate(arrowPrefab, firePoint.position, Quaternion.Euler(0, 0, aimAngle));
        }
        isAttacking = false;
    }

    void SpawnTimeSphere(float angle)
    {
        GameObject sphere = Instantiate(sphereVisualPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        var logic = sphere.AddComponent<TimeSphereLogic>();

        // Передаем настройки из Инспектора прямо в сферу!
        logic.speed = sphereSpeed;
        logic.stunDuration = sphereStunTime;
    }

    IEnumerator Phase3_UltimateAttack()
    {
        isAttacking = true;
        if (lightRainVisualPrefab == null) { isAttacking = false; yield break; }

        Color oldColor = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = Color.yellow; // Босс кастует

        for (int wave = 0; wave < ultWaveCount; wave++)
        {
            for (int i = 0; i < ultCirclesPerWave; i++)
            {
                // Раскидываем круги ШИРОКО вокруг игрока (от 2 до ultSpreadRadius метров)
                Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(2f, ultSpreadRadius);
                Vector3 rainPos = player.position + (Vector3)randomOffset;

                GameObject rain = Instantiate(lightRainVisualPrefab, rainPos, Quaternion.identity);
                var logic = rain.AddComponent<LightRainLogic>();

                // Передаем настройки из Инспектора в круг!
                logic.delay = ultExplosionDelay;
                logic.radius = ultExplosionRadius;
                logic.damage = ultDamage;
            }
            yield return new WaitForSeconds(ultExplosionDelay + 0.3f); // Ждем взрыва волны + чуть-чуть
        }

        GetComponent<SpriteRenderer>().color = oldColor;
        nextUltTime = Time.time + ultCooldown;
        isAttacking = false;
    }

    // Рисуем границы Арены в редакторе (Голубой прямоугольник)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Transform center = (arenaCenter != null) ? arenaCenter : transform;
        Gizmos.DrawWireCube(center.position, new Vector3(arenaSize.x, arenaSize.y, 1f));
    }
}

// ==========================================
// ЛОГИКА СФЕРЫ ВРЕМЕНИ (Стан)
// ==========================================
public class TimeSphereLogic : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public float stunDuration;

    void Start() => Destroy(gameObject, 6f);
    void Update() => transform.Translate(Vector3.up * speed * Time.deltaTime);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Health>()?.TakeDamage(1);
            PlayerController2D pc = other.GetComponent<PlayerController2D>();
            if (pc != null) pc.StartCoroutine(StunPlayer(pc));
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator StunPlayer(PlayerController2D pc)
    {
        float oldSpeed = pc.moveSpeed;
        pc.moveSpeed = 0f;
        yield return new WaitForSeconds(stunDuration);
        if (pc != null) pc.moveSpeed = oldSpeed;
    }
}

// ==========================================
// ЛОГИКА ГРАДА СВЕТА (Ульта 3-й фазы)
// ==========================================
public class LightRainLogic : MonoBehaviour
{
    [HideInInspector] public float delay;
    [HideInInspector] public float radius;
    [HideInInspector] public float damage;

    void Start()
    {
        // Подгоняем размер визуального круга под радиус урона
        transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(delay);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                h.GetComponent<Health>()?.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}