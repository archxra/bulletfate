using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Pathfinder : MonoBehaviour
{
    public float speed = 4f;
    public float predictionTime = 0.5f;
    public float wobbleStrength = 0.5f; // Уменьшил для плавности

    private Transform player;
    private Rigidbody2D rb;
    private List<Vector3> path;
    private int waypointIndex = 0;
    private Vector3 smoothedPredictedPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        InvokeRepeating("UpdatePath", 0f, 0.3f);
    }

    void UpdatePath()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;
        player = p.transform;

        Vector3 playerVelocity = Vector3.zero;
        var pRb = player.GetComponent<Rigidbody2D>();
        if (pRb != null) playerVelocity = pRb.linearVelocity;

        // СГЛАЖИВАНИЕ ПРЕДСКАЗАНИЯ: чтобы точка не прыгала
        Vector3 targetPos = player.position + (playerVelocity * predictionTime);
        smoothedPredictedPos = Vector3.Lerp(smoothedPredictedPos, targetPos, 0.2f);

        if (AStar2D.Instance != null)
        {
            path = AStar2D.Instance.FindPath(transform.position, smoothedPredictedPos);
            waypointIndex = 0;
        }
    }

    void FixedUpdate()
    {


        if (path == null || waypointIndex >= path.Count) return;

        Vector3 target = path[waypointIndex];
        Vector2 dir = (target - transform.position).normalized;

        // ПЛАВНОЕ ДВИЖЕНИЕ
        Vector2 wobble = new Vector2(-dir.y, dir.x) * Mathf.Sin(Time.time * 3f) * wobbleStrength;
        Vector2 finalVelocity = (dir + wobble).normalized * speed;

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, finalVelocity, 0.1f);

        if (Vector2.Distance(transform.position, target) < 0.3f) waypointIndex++;
    }
}