using UnityEngine;
using System.Collections;

public class Boss_Spider_Master : MonoBehaviour
{
    [Header("Движение")]
    public float speed = 4f;
    public float dashSpeed = 24f;        // Было 18 → стало 24
    public float dashDuration = 0.3f;    // Было 0.2 → дольше летит = дальше
    public float preferredDistance = 4f;
    public float contactDamageDistance = 1.5f;
    public float damage = 5f;

    [Header("Стрельба")]
    public GameObject webPrefab;
    public Transform firePoint;
    public float bulletSpeed = 14f;      // Было 10 → стало 14
    public float shootDelay = 0.35f;     // Было 0.5 → стало 0.35

    [Header("Призыв мобов")]
    public GameObject[] minionPrefabs;
    public float summonRadius = 5f;
    public int minMinions = 1;
    public int maxMinions = 3;
    public float summonCooldown = 15f;

    [Header("Ссылки")]
    public Animator anim;

    private Rigidbody2D rb;
    private Transform player;
    private int lastAttack = -1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(BossLogicLoop());
        StartCoroutine(SummonTimer());
    }

    IEnumerator SummonTimer()
    {
        yield return new WaitForSeconds(summonCooldown);
        while (true)
        {
            yield return StartCoroutine(SummonMinions());
            yield return new WaitForSeconds(summonCooldown);
        }
    }

    IEnumerator BossLogicLoop()
    {
        while (true)
        {
            yield return StartCoroutine(WanderRoutine());

            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isMoving", false);

            int nextAttack;
            do
            {
                nextAttack = Random.Range(0, 2);
            } while (nextAttack == lastAttack);
            lastAttack = nextAttack;

            if (nextAttack == 0)
                yield return StartCoroutine(ShootSeries());
            else
                yield return StartCoroutine(DashSeries());
        }
    }

    IEnumerator WanderRoutine()
    {
        float timer = 0;
        float wanderTime = Random.Range(2f, 4f);

        while (timer < wanderTime)
        {
            if (player == null) { yield return null; continue; }

            float dist = Vector2.Distance(transform.position, player.position);
            Vector2 dir;

            if (dist > preferredDistance + 0.5f)
                dir = (player.position - transform.position).normalized;
            else if (dist < preferredDistance - 0.5f)
                dir = (transform.position - player.position).normalized;
            else
                dir = Vector2.zero;

            rb.linearVelocity = dir * speed;
            anim.SetBool("isMoving", dir != Vector2.zero);
            GetComponent<SpriteRenderer>().flipX = (player.position.x - transform.position.x < 0);

            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        rb.linearVelocity = Vector2.zero;
    }

    IEnumerator ShootSeries()
    {
        int shots = Random.Range(2, 7);
        for (int i = 0; i < shots; i++)
        {
            if (player == null) yield break;

            anim.Play("shoot");
            yield return new WaitForSeconds(shootDelay); // 0.35s

            Vector2 dir = (player.position - transform.position).normalized;
            GameObject bullet = Instantiate(webPrefab, firePoint.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().linearVelocity = dir * bulletSpeed; // 14f

            yield return new WaitForSeconds(shootDelay); // 0.35s пауза между выстрелами
        }
    }

    IEnumerator DashSeries()
    {
        int dashes = Random.Range(2, 7);
        anim.Play("prepare");
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < dashes; i++)
        {
            if (player == null) yield break;

            anim.Play("dash");
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * dashSpeed; // 24f

            bool hitPlayer = false;
            float dashTimer = 0f;

            while (dashTimer < dashDuration) // 0.3s — дольше летит = дальше
            {
                if (!hitPlayer)
                {
                    float dist = Vector2.Distance(transform.position, player.position);
                    if (dist <= contactDamageDistance)
                    {
                        player.GetComponent<Health>()?.TakeDamage(damage);
                        hitPlayer = true;
                    }
                }
                dashTimer += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.25f); // чуть короче пауза между дэшами
        }
    }

    IEnumerator SummonMinions()
    {
        if (minionPrefabs == null || minionPrefabs.Length == 0) yield break;

        yield return new WaitForSeconds(0.8f);

        int count = Random.Range(minMinions, maxMinions + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * summonRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
            Instantiate(prefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }
    }
}