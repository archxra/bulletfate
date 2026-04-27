using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_AStarShooter : MonoBehaviour
{
    [Header("��������� ��������")]
    public float speed = 3f;           // ������� ��������
    public float retreatSpeed = 5.5f;  // ��������, ����� ���� ������� �� ����
    public float attackDistance = 7f;  // ������ ���������� ��������
    public float retreatDistance = 4f; // ���� ����� ����� ����� - ���� �������� � �����
    public LayerMask obstacleLayer;    // ���� ���� (Obstacle)

    [Header("��������")]
    public GameObject bulletPrefab;    // ������ ������� ����
    public Transform firePoint;        // ����� ������ ���� (������ ������ ����� ������)
    public float fireRate = 1.5f;      // ����� ����� ����������

    private Transform player;
    private Rigidbody2D rb;
    private EnemyContactDamage contactDamage;

    private List<Vector3> path;
    private int currentWaypointIndex = 0;
    private float nextFireTime = 0f;
    private int strafeDir = 1;         // ����������� ����� (1 ��� -1)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        contactDamage = GetComponent<EnemyContactDamage>();

        // ������ 0.4 ��� ��������� �������
        InvokeRepeating("UpdatePath", 0f, 0.4f);
        // ������ 2 ��� ������ ����������� ���� �� �����
        InvokeRepeating("ChangeStrafeDir", 2f, 2f);
    }

    void ChangeStrafeDir() { strafeDir *= -1; }

    // ���� ����� ����������, ����� ������ ����������� (��������, ��������� ��� ����)
    void OnDisable()
    {
        path = null; // ���������� ����
        if (rb != null) rb.linearVelocity = Vector2.zero; // ������������� ������
    }

    void UpdatePath()
    {
        // ���� ������ (�� ����� �������� ��� ������������ ����������)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (player == null || AStar2D.Instance == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 dirToPlayer = (player.position - transform.position).normalized;

        // �������� ������ (����� �� ������ ������ �����)
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, dirToPlayer, distance, obstacleLayer);
        bool canSeePlayer = (wallHit.collider == null);

        // ������ ����� � ���� Scene ��� �������
        Debug.DrawLine(transform.position, player.position, canSeePlayer ? Color.green : Color.red);

        Vector3 targetPosition;

        if (!canSeePlayer || distance > attackDistance)
        {
            // 1. �� ����� ��� ������: ���� � ������ �� ����������� ����
            targetPosition = player.position;
        }
        else if (distance < retreatDistance)
        {
            // 2. ������� ������: ������� � ����� ������ ����
            targetPosition = transform.position - (Vector3)dirToPlayer * 5f;
        }
        else
        {
            // 3. �����: ������ ������ ������ �� ����
            Vector3 sideDir = Quaternion.Euler(0, 0, 45 * strafeDir) * (-dirToPlayer);
            targetPosition = player.position + sideDir * attackDistance;
        }

        // ����������� ���� � "�����" A*
        path = AStar2D.Instance.FindPath(transform.position, targetPosition);
        currentWaypointIndex = 0;
    }

    void Update()
    {
        // ���� ������ �������� ������ - ������ �� ������
        if (!enabled || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 dirToPlayer = (player.position - transform.position).normalized;

        // �������� ��������� ��� ��������
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, dirToPlayer, distance, obstacleLayer);

        if (wallHit.collider == null && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // ���� FirePoint �� ��������, �������� �� ������
        Transform spawnPoint = (firePoint != null) ? firePoint : transform;
        Instantiate(bulletPrefab, spawnPoint.position, Quaternion.Euler(0, 0, angle));
    }

    void FixedUpdate()
    {
        // ПРОВЕРКА НА СТАН
        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (player == null || path == null || path.Count == 0) return;

        // 1. Считаем Расталкивание
        Vector2 separation = Vector2.zero;
        float neighborRadius = 1f;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, neighborRadius);
        foreach (var n in neighbors)
        {
            if (n.gameObject != gameObject && n.CompareTag("Enemy"))
            {
                Vector2 diff = (Vector2)transform.position - (Vector2)n.transform.position;
                separation += diff.normalized / (diff.magnitude + 0.1f);
            }
        }

        // 2. Логика движения по пути
        if (currentWaypointIndex < path.Count)
        {
            Vector3 targetWaypoint = path[currentWaypointIndex];

            // Направление к точке пути
            Vector2 moveDir = (targetWaypoint - transform.position).normalized;

            // Определяем текущую скорость (обычная или побег)
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            float currentMoveSpeed = (distToPlayer < retreatDistance) ? retreatSpeed : speed;

            // 3. СМЕШИВАЕМ направление пути и расталкивание
            Vector2 finalDir = (moveDir + separation * 0.5f).normalized;

            // 4. Двигаем
            rb.MovePosition(rb.position + finalDir * currentMoveSpeed * Time.fixedDeltaTime);

            if (Vector2.Distance(transform.position, targetWaypoint) < 0.2f)
                currentWaypointIndex++;
        }
    }
}
