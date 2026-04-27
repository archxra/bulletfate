using UnityEngine;
using System.Collections;

public class Boss_Spider_Skills : MonoBehaviour
{
    [Header("�������� (4 ����, �� 4�)")]
    public GameObject webPrefab;
    public Transform firePoint;
    public float bulletSpeed = 12f;

    [Header("������� (3 �����)")]
    public float meleeRange = 3.5f;
    public float meleeDamage = 3f;

    private Transform player;
    private Rigidbody2D playerRb;
    private bool isAttacking = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerRb = p.GetComponent<Rigidbody2D>();
        }
        StartCoroutine(BossLogicLoop());
    }

    IEnumerator BossLogicLoop()
    {
        while (true)
        {
            if (player == null) yield break;

            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= meleeRange)
            {
                // ���� ����� ������ � ������� � ����
                yield return StartCoroutine(MeleeSlam());
            }
            else
            {
                // ���� ����� ������ - �������������
                yield return StartCoroutine(RangedBurst());
                yield return new WaitForSeconds(4f); // �� ����� ������� ���������
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator RangedBurst()
    {
        isAttacking = true;
        for (int i = 0; i < 4; i++)
        {
            if (player == null) break;
            ShootPredicted();
            yield return new WaitForSeconds(0.8f);
        }
        isAttacking = false;
    }

    void ShootPredicted()
    {
        Vector2 pVel = (playerRb != null) ? playerRb.linearVelocity : Vector2.zero;
        float dist = Vector2.Distance(transform.position, player.position);
        float time = dist / bulletSpeed;

        // ������� � �����, ��� ����� ����� ����� ����� ������ ����
        Vector2 predictedPos = (Vector2)player.position + (pVel * time);
        Vector2 dir = (predictedPos - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Instantiate(webPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
    }

    IEnumerator MeleeSlam()
    {
        isAttacking = true;
        Debug.Log("�������: ���� �� �����!");

        // ������ ������ (�������������)
        transform.localScale *= 1.2f;
        yield return new WaitForSeconds(0.3f);

        // ����
        if (Vector2.Distance(transform.position, player.position) <= meleeRange)
        {
            player.GetComponent<Health>()?.TakeDamage(meleeDamage);
        }

        transform.localScale /= 1.2f;
        yield return new WaitForSeconds(1.5f); // �� ��������� �����
        isAttacking = false;
    }
}