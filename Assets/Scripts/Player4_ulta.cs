using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Mage_UltimateCircle : MonoBehaviour
{
    public GameObject visual;
    public float radius = 5f;
    public float damage = 3f;
    public float duration = 10f;
    public float cooldown = 90f;
    public LayerMask enemyLayer;

    private float nextUse;

    void Update()
    {
        // ПКМ или R (как тебе удобнее, давай оставим R для ульты)
        if (Keyboard.current.rKey.wasPressedThisFrame && Time.time >= nextUse)
        {
            StartCoroutine(ActivateCircle());
            nextUse = Time.time + cooldown;
        }
    }

    IEnumerator ActivateCircle()
    {
        if (visual != null)
        {
            visual.SetActive(true);
            visual.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
        }

        float elapsed = 0;
        while (elapsed < duration)
        {
            ApplyEffect();
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        if (visual != null) visual.SetActive(false);
    }

    void ApplyEffect()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var t in targets)
        {
            // 1. Урон
            Health hp = t.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(damage);

            // 2. УНИВЕРСАЛЬНОЕ ЗАМЕДЛЕНИЕ
            // Мы просто ищем ЛЮБОЙ скрипт на враге, у которого есть переменная "speed"
            // Это сработает и для EnemyAI_Simple, и для боссов, и для старых скриптов.
            MonoBehaviour[] allScripts = t.GetComponents<MonoBehaviour>();
            foreach (var s in allScripts)
            {
                // Если название скрипта содержит "AI" или "Boss" или "Simple"
                string n = s.GetType().Name;
                if (n.Contains("Enemy") || n.Contains("AI") || n.Contains("Boss") || n.Contains("Simple"))
                {
                    StartCoroutine(SlowAnyAI(s));
                }
            }
        }
    }

    // Эта штука замедляет ЛЮБОЙ скрипт, если в нем есть поле 'speed'
    IEnumerator SlowAnyAI(MonoBehaviour script)
    {
        var field = script.GetType().GetField("speed");
        if (field != null)
        {
            float originalSpeed = (float)field.GetValue(script);
            field.SetValue(script, originalSpeed * 0.5f); // Замедляем на 50%

            yield return new WaitForSeconds(1.1f);

            if (script != null) // Возвращаем скорость назад
                field.SetValue(script, originalSpeed);
        }
    }
}