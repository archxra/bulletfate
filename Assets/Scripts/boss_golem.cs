using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Golem_Complete : MonoBehaviour
{
    [Header("=== �������� ===")]
    public float speed = 2f; // ����� ���������
    public float rotationSpeed = 5f;
    public LayerMask obstacleLayer;

    [Header("=== ��������� ===")]
    public float shootDistance = 8f;
    public float meleeDistance = 3.5f;

    [Header("=== ����� 1: ����� (��������) ===")]
    public GameObject rockPrefab;
    public Transform firePoint;
    public float rockCooldown = 1.5f; // �� ��

    [Header("=== ����� 2: ���� �� ����� (�������) ===")]
    public float slamDamage = 5f;
    public float slamCooldown = 4f;
    public float slamCastTime = 0.5f;
    public GameObject slamWarningVisual; // ������� ���� ������ ������

    [Header("=== ����� 3: ������� �� ���������� ===")]
    public GameObject crystalTrapPrefab; // ������ ������� (������� �� ������� ����)
    public float trapCooldown = 8f;
    public float trapCastTime = 1f;

    // ���������
    private Transform player;
    private Rigidbody2D rb;
    private List<Vector3> path;
    private int waypointIndex = 0;

    // ���������
    private bool isAttacking = false;
    private float nextRockTime = 0f;
    private float nextSlamTime = 0f;
    private float nextTrapTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (slamWarningVisual != null) slamWarningVisual.SetActive(false);
        InvokeRepeating("ThinkAndPathfind", 0f, 0.5f);
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

        // ���� ������ - ���� � ������. ����� �� �������, �� ����!
        if (!canSeePlayer || dist > shootDistance)
        {
            targetPos = player.position;
            path = AStar2D.Instance.FindPath(transform.position, targetPos);
            waypointIndex = 0;
        }
        else
        {
            path = null; // ����� � ����
        }
    }

    void Update()
    {
        if (!enabled || player == null || isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D wallHit = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
        bool canSeePlayer = (wallHit.collider == null);

        // ��������� 1: ���� �� ����� (�������� ������)
        if (dist <= meleeDistance && Time.time >= nextSlamTime)
        {
            StartCoroutine(GroundSlamAttack());
        }
        // ��������� 2: ������� �� ���������� (��������� �� ������)
        else if (canSeePlayer && Time.time >= nextTrapTime)
        {
            StartCoroutine(CrystalTrapAttack());
        }
        // ��������� 3: ������ �������� �����
        else if (canSeePlayer && dist <= shootDistance && Time.time >= nextRockTime)
        {
            ShootRock();
            nextRockTime = Time.time + rockCooldown;
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

            rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

            if (rb.linearVelocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), rotationSpeed * Time.fixedDeltaTime);
            }

            if (Vector2.Distance(transform.position, target) < 0.2f) waypointIndex++;
        }
    }

    // --- ����� ---

    void ShootRock()
    {
        if (rockPrefab == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Transform fp = (firePoint != null) ? firePoint : transform;
        Instantiate(rockPrefab, fp.position, Quaternion.Euler(0, 0, angle));
    }

    IEnumerator CrystalTrapAttack()
    {
        isAttacking = true;
        path = null;
        rb.linearVelocity = Vector2.zero;

        Debug.Log("�����: ���� ����� ����� (�������)!");

        // �������� ������
        Vector3 origScale = transform.localScale;
        transform.localScale = new Vector3(origScale.x * 1.2f, origScale.y * 1.2f, origScale.z);

        yield return new WaitForSeconds(trapCastTime);

        if (player != null && crystalTrapPrefab != null)
        {
            // ������� ������� ����� �� ������
            Instantiate(crystalTrapPrefab, player.position, Quaternion.identity);
        }

        transform.localScale = origScale;
        nextTrapTime = Time.time + trapCooldown;

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    IEnumerator GroundSlamAttack()
    {
        isAttacking = true;
        path = null;
        rb.linearVelocity = Vector2.zero;

        Debug.Log("�����: ���� ����� ������!");

        if (slamWarningVisual != null) slamWarningVisual.SetActive(true);

        // ����� ����������
        Vector3 origScale = transform.localScale;
        transform.localScale = new Vector3(origScale.x * 1.3f, origScale.y * 1.3f, origScale.z);

        yield return new WaitForSeconds(slamCastTime);

        // ����
        if (player != null && Vector2.Distance(transform.position, player.position) <= meleeDistance)
        {
            player.GetComponent<Health>()?.TakeDamage(slamDamage);
        }

        if (slamWarningVisual != null) slamWarningVisual.SetActive(false);
        transform.localScale = origScale;

        nextSlamTime = Time.time + slamCooldown;
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, shootDistance);
    }
}