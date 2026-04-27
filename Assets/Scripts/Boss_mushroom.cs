using UnityEngine;
using System.Collections;

public class Boss_Mushroom_Complete : MonoBehaviour
{
    [Header("=== СИСТЕМНЫЕ ===")]
    [Tooltip("Точка, откуда вылетают шипы")]
    public Transform firePoint;

    [Header("=== АТАКА 1: Дробовик (Шипы) ===")]
    public GameObject spikePrefab;
    [Tooltip("Задержка между выстрелами дробовика")]
    public float shootInterval = 1f;
    [Tooltip("Сколько шипов вылетает за один выстрел")]
    public int spikeCount = 3;
    [Tooltip("Угол разброса между крайними шипами (например, 40 градусов)")]
    public float spreadAngle = 40f;

    [Header("=== АТАКА 2: Призыв Зомби ===")]
    public GameObject zombiePrefab;
    [Tooltip("Как часто босс спавнит помощников (в секундах)")]
    public float summonInterval = 25f;
    [Tooltip("Сколько зомби появляется за один раз")]
    public int summonCount = 3;
    [Tooltip("В каком радиусе от гриба появляются зомби")]
    public float summonRadius = 2.5f;

    [Header("=== АТАКА 3: Облако Спор (Вблизи) ===")]
    [Tooltip("Полупрозрачный круг, показывающий зону заражения")]
    public GameObject sporeVisual;
    [Tooltip("Радиус поражения спор")]
    public float sporeRadius = 4f;
    [Tooltip("Урон, который споры наносят КАЖДУЮ секунду")]
    public float sporeDamagePerSecond = 1f;
    [Tooltip("Сколько секунд висит облако спор")]
    public float sporeDuration = 5f;
    [Tooltip("Перезарядка способности после того, как облако исчезнет")]
    public float sporeCooldown = 20f;

    private Transform player;
    private float nextShootTime;
    private float nextSummonTime;
    private float nextSporeTime;

    void Start()
    {
        if (sporeVisual != null) sporeVisual.SetActive(false);

        // Первый призыв зомби откладываем на summonInterval, чтобы босс не спавнил их сразу на старте
        nextSummonTime = Time.time + summonInterval;
    }

    void Update()
    {
        // Постоянно ищем игрока (на случай переключения 1-2-3-4-5)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        else return;

        // 1. Атака Дробовиком (по таймеру)
        if (Time.time >= nextShootTime)
        {
            ShootShotgun();
            nextShootTime = Time.time + shootInterval;
        }

        // 2. Призыв Зомби (по таймеру)
        if (Time.time >= nextSummonTime)
        {
            SummonZombies();
            nextSummonTime = Time.time + summonInterval;
        }

        // 3. Облако Спор (по дистанции и таймеру)
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= sporeRadius && Time.time >= nextSporeTime)
        {
            StartCoroutine(SporeCloudRoutine());
            // Кулдаун начинается сразу, но включает в себя время работы облака
            nextSporeTime = Time.time + sporeCooldown + sporeDuration;
        }
    }

    // --- ЛОГИКА АТАК ---

    void ShootShotgun()
    {
        if (spikePrefab == null || firePoint == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // Вычисляем угол между шипами в зависимости от их количества
        float angleStep = spreadAngle / (spikeCount > 1 ? spikeCount - 1 : 1);
        float startingAngle = baseAngle - (spreadAngle / 2f);

        for (int i = 0; i < spikeCount; i++)
        {
            float currentAngle = startingAngle + (angleStep * i);
            Instantiate(spikePrefab, firePoint.position, Quaternion.Euler(0, 0, currentAngle));
        }
    }

    void SummonZombies()
    {
        if (zombiePrefab == null) return;
        Debug.Log("ГРИБ: Призываю зомби!");

        for (int i = 0; i < summonCount; i++)
        {
            // Спавним в случайной точке вокруг босса
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * summonRadius;
            Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
        }
    }

    IEnumerator SporeCloudRoutine()
    {
        Debug.Log("ГРИБ: Выпускаю облако спор!");
        if (sporeVisual != null) sporeVisual.SetActive(true);

        float elapsed = 0f;
        while (elapsed < sporeDuration)
        {
            // Проверяем каждую секунду, стоит ли игрок в зоне спор
            if (player != null && Vector2.Distance(transform.position, player.position) <= sporeRadius)
            {
                player.GetComponent<Health>()?.TakeDamage(sporeDamagePerSecond);
            }

            yield return new WaitForSeconds(1f); // Урон тикает 1 раз в секунду
            elapsed += 1f;
        }

        if (sporeVisual != null) sporeVisual.SetActive(false);
    }

    // Рисуем радиусы в редакторе (зеленый = Споры, желтый = зона спавна зомби)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, sporeRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
    }
}