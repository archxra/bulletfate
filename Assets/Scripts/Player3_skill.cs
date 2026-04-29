using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Skill_WarCry : MonoBehaviour
{
    public float radius = 5f;
    public float duration = 3f;
    public float cooldown = 15f;
    public LayerMask enemyLayer;
    private float nextUse;

    void Update()
    {
        // ПКМ для крика
        if (Mouse.current.rightButton.wasPressedThisFrame && Time.time >= nextUse)
        {
            Cry();
            nextUse = Time.time + cooldown;
        }
    }

    void Cry()
    {
        Debug.Log("ГЛАДИАТОР ОРЕТ!");
        // Ищем всех врагов в радиусе
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        foreach (Collider2D e in enemies)
        {
            StartCoroutine(StunEnemy(e.gameObject));
        }
    }

    IEnumerator StunEnemy(GameObject enemy)
    {
        if (enemy == null) yield break;

        // Берем все скрипты, которые висят на враге
        MonoBehaviour[] allScripts = enemy.GetComponents<MonoBehaviour>();
        SpriteRenderer sprite = enemy.GetComponent<SpriteRenderer>();

        // Запоминаем, какие скрипты мы выключили, чтобы потом включить только их
        List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();

        foreach (var s in allScripts)
        {
            // Выключаем всё, КРОМЕ скрипта здоровья (Health) и самого этого корутин-менеджера
            // Мы ищем скрипты ИИ по ключевым словам в названиях
            string scriptName = s.GetType().Name.ToLower();
            if (scriptName.Contains("ai") || scriptName.Contains("simple") || scriptName.Contains("path") || scriptName.Contains("damage"))
            {
                if (s.enabled)
                {
                    s.enabled = false;
                    disabledScripts.Add(s);
                }
            }
        }

        // Визуал стана
        Color oldColor = Color.white;
        if (sprite != null)
        {
            oldColor = sprite.color;
            sprite.color = Color.gray;
        }

        // Ждем 3 секунды
        yield return new WaitForSeconds(duration);

        // Возвращаем всё как было
        if (enemy != null)
        {
            foreach (var s in disabledScripts)
            {
                if (s != null) s.enabled = true;
            }
            if (sprite != null) sprite.color = oldColor;
        }
    }
}