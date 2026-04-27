using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Skill_WarCry : MonoBehaviour
{
    public float radius = 5f;
    public float duration = 3f; // 3 сек стан по документу
    public float cooldown = 15f; // 15 сек кд по документу
    public LayerMask enemyLayer;
    private float nextUse;

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame && Time.time >= nextUse)
        {
            Cry();
            nextUse = Time.time + cooldown;
        }
    }

    void Cry()
    {
        Debug.Log("Гладиатор ОРЕТ!");
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (Collider2D e in enemies)
        {
            StartCoroutine(StunEnemy(e.gameObject));
        }
    }

    IEnumerator StunEnemy(GameObject enemy)
    {
        // Находим все скрипты на враге
        var aiMelee = enemy.GetComponent<EnemyAI_Pathfinder>();
        var aiShooter = enemy.GetComponent<EnemyAI_AStarShooter>();
        var contactDamage = enemy.GetComponent<EnemyContactDamage>();
        var sprite = enemy.GetComponent<SpriteRenderer>();

        // Выключаем всё!
        if (aiMelee != null) aiMelee.enabled = false;
        if (aiShooter != null) aiShooter.enabled = false;
        if (contactDamage != null) contactDamage.enabled = false;

        Color oldColor = sprite.color;
        sprite.color = Color.gray;

        yield return new WaitForSeconds(duration);

        // Включаем обратно
        if (aiMelee != null) aiMelee.enabled = true;
        if (aiShooter != null) aiShooter.enabled = true;
        if (contactDamage != null) contactDamage.enabled = true;
        sprite.color = oldColor;
    }
}