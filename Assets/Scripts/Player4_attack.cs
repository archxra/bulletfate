using UnityEngine;
using UnityEngine.InputSystem;

public class MageCombat : MonoBehaviour
{
    [Header("Обычная атака (Фаербол)")]
    public GameObject fireballPrefab;
    public float attackCooldown = 0.7f;
    private float nextAttackTime;

    public Transform firePoint;

    void Update()
    {
        // ЛКМ - Фаербол
        if (Mouse.current.leftButton.isPressed && Time.time >= nextAttackTime)
        {
            Instantiate(fireballPrefab, firePoint.position, transform.rotation);
            nextAttackTime = Time.time + attackCooldown;
        }
    }
}