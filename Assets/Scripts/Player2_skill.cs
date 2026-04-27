using UnityEngine;
using UnityEngine.InputSystem;

public class Skill_LaserSword : MonoBehaviour
{
    [Header("Настройки удара")]
    public float damage = 8f;
    public float cooldown = 15f;
    public float slashRadius = 4f;   // Дальность удара (радиус круга)
    [Range(0, 360)]
    public float slashAngle = 120f;  // Угол удара (сектор)
    public LayerMask enemyLayer;

    [Header("Визуал")]
    public GameObject slashVisualPrefab; // Сюда закинем префаб вспышки

    private float nextUseTime;

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame && Time.time >= nextUseTime)
        {
            PerformArcSlash();
            nextUseTime = Time.time + cooldown;
        }
    }

    void PerformArcSlash()
    {
        Debug.Log("УДАР МЕЧОМ (ВЕЕР)!");

        // 1. Создаем визуальный эффект (кружок, который мы растянем)
        if (slashVisualPrefab != null)
        {
            GameObject visual = Instantiate(slashVisualPrefab, transform.position, transform.rotation);
            // Делаем визуальный эффект размером с радиус удара
            visual.transform.localScale = new Vector3(slashRadius * 2, slashRadius * 2, 1);
        }

        // 2. Ищем всех врагов в радиусе круга вокруг игрока
        Collider2D[] enemiesInRadius = Physics2D.OverlapCircleAll(transform.position, slashRadius, enemyLayer);

        foreach (Collider2D enemy in enemiesInRadius)
        {
            // Считаем направление к врагу
            Vector2 directionToEnemy = (enemy.transform.position - transform.position).normalized;

            // Считаем угол между направлением взгляда игрока (transform.up) и врагом
            float angleToEnemy = Vector2.Angle(transform.up, directionToEnemy);

            // Если угол меньше половины нашего сектора (60 градусов в каждую сторону от центра)
            if (angleToEnemy <= slashAngle / 2f)
            {
                Health h = enemy.GetComponent<Health>();
                if (h != null) h.TakeDamage(damage);
            }
        }
    }

    // Рисуем веер в редакторе (Gizmos)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, -slashAngle / 2f) * transform.up;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, slashAngle / 2f) * transform.up;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * slashRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * slashRadius);
        // Рисуем примерный круг
#if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(0, 0, 1, 0.1f);
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.forward, leftBoundary, slashAngle, slashRadius);
#endif
    }
}