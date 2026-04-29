using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public PhysicsMaterial2D bouncyMaterial;
    private bool isDead = false;

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. ОТКЛЮЧАЕМ ВСЕ СКРИПТЫ УПРАВЛЕНИЯ
        // Перечисляем все скрипты, которые отвечают за игру и стрельбу
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // Отключаем всё, что управляет игроком
            if (script is PlayerController2D || script is PlayerAttack2D || script is WeaponAim)
            {
                script.enabled = false;
            }
        }

        // 2. Дополнительно отключаем Pivot с оружием, чтобы оно точно замерло
        Transform weapon = transform.Find("WeaponPivot");
        if (weapon != null)
        {
            weapon.gameObject.SetActive(false); // Прячем ружье полностью
        }

        // 3. ФИЗИКА ТРУПА
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1.0f;

        // ВАЖНО: Убираем заморозку вращения, чтобы труп мог кувыркаться
        rb.constraints = RigidbodyConstraints2D.None;

        // Применяем прыгучий материал
        GetComponent<Collider2D>().sharedMaterial = bouncyMaterial;

        // Импульс падения
        rb.AddForce(new Vector2(Random.Range(-2f, 2f), 5f), ForceMode2D.Impulse);
        rb.angularVelocity = Random.Range(-200f, 200f);

        Debug.Log("Герой мертв, управление отключено.");
    }
}