using UnityEngine;

public class EnemyAI_SimpleShooter : MonoBehaviour
{
    public float speed = 3f;
    public float attackDistance = 7f;
    public float detectionRadius = 20f;
    public LayerMask obstacleLayer;
    public GameObject bulletPrefab;
    public float fireRate = 1.5f;

    private Transform player;
    private Rigidbody2D rb;
    private float nextFire;

    void Start() => rb = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 dir = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleLayer);

        if (dist < detectionRadius && hit.collider == null)
        {
            // ВИДИМ ИГРОКА
            if (dist > attackDistance)
            {
                rb.linearVelocity = dir * speed; // Идем сближаться
            }
            else
            {
                // МАНСЫ (простое движение боком)
                Vector2 sideDir = new Vector2(-dir.y, dir.x);
                rb.linearVelocity = sideDir * speed * 0.5f;

                if (Time.time > nextFire)
                {
                    Shoot(dir);
                    nextFire = Time.time + fireRate;
                }
            }

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Shoot(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Instantiate(bulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));
    }
}