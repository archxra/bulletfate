using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    public float damage = 1f;
    public float damageCooldown = 1f;

    private float nextDamageTime = 0f;

    private void OnTriggerStay2D(Collider2D other)
    {
        // Если скрипт выключен (например, враг в Стане), урон не наносим
        if (!this.enabled) return;

        // Проверяем, коснулись ли мы Игрока
        if (other.CompareTag("Player") && Time.time >= nextDamageTime)
        {
            Health playerHealth = other.GetComponent<Health>();

            // Если игрок найден и он НЕ бессмертен
            if (playerHealth != null && !playerHealth.isInvincible)
            {
                playerHealth.TakeDamage(damage);
                nextDamageTime = Time.time + damageCooldown;
                Debug.Log("Враг нанес контактный урон: " + damage);
            }
        }
    }
}