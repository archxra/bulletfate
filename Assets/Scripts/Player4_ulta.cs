using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Mage_UltimateCircle : MonoBehaviour
{
    public GameObject visual; // Дочерний фиолетовый круг игрока
    public float radius = 5f;
    public float damage = 3f;
    public float duration = 10f;
    public float cooldown = 90f;
    public LayerMask enemyLayer;

    private float nextUse;

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && Time.time >= nextUse)
        {
            StartCoroutine(ActivateCircle());
            nextUse = Time.time + cooldown;
        }
    }

    IEnumerator ActivateCircle()
    {
        visual.SetActive(true);
        visual.transform.localScale = new Vector3(radius * 2, radius * 2, 1);

        float elapsed = 0;
        while (elapsed < duration)
        {
            ApplyEffect();
            yield return new WaitForSeconds(1f); // Тик раз в секунду
            elapsed += 1f;
        }
        visual.SetActive(false);
    }

    void ApplyEffect()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var t in targets)
        {
            Health hp = t.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(damage);

            // Проверяем милишника
            EnemyAI_Pathfinder aiMelee = t.GetComponent<EnemyAI_Pathfinder>();
            if (aiMelee != null) StartCoroutine(SlowMelee(aiMelee));

            // Проверяем стрелка отдельно
            EnemyAI_AStarShooter aiShooter = t.GetComponent<EnemyAI_AStarShooter>();
            if (aiShooter != null) StartCoroutine(SlowShooter(aiShooter));
        }
    }

    // Две разные корутины для двух разных типов скриптов
    IEnumerator SlowMelee(EnemyAI_Pathfinder ai)
    {
        float oldSpeed = ai.speed;
        ai.speed *= 0.5f;
        yield return new WaitForSeconds(1.1f);
        if (ai != null) ai.speed = oldSpeed;
    }

    IEnumerator SlowShooter(EnemyAI_AStarShooter ai)
    {
        float oldSpeed = ai.speed;
        ai.speed *= 0.5f;
        yield return new WaitForSeconds(1.1f);
        if (ai != null) ai.speed = oldSpeed;
    }
}
