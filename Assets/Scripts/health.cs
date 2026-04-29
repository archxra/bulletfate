using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;

    private SpriteRenderer sprite;
    private Color originalColor;

    [HideInInspector] public bool isInvincible = false;
    private Coroutine flashRoutine;
    private Coroutine burnRoutine;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) originalColor = sprite.color;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvincible) return;

        currentHealth -= damageAmount;

        if (sprite != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0) Die();
    }

    // Ìåòîä äëÿ Ãîðåíèÿ
    public void ApplyBurn(float tickDamage, float duration)
    {
        if (isInvincible) return;
        if (burnRoutine != null) StopCoroutine(burnRoutine);
        burnRoutine = StartCoroutine(BurnProcess(tickDamage, duration));
    }

    IEnumerator BurnProcess(float dmg, float dur)
    {
        float elapsed = 0;
        if (sprite != null) sprite.color = new Color(1f, 0.5f, 0f); // Îðàíæåâûé

        while (elapsed < dur)
        {
            yield return new WaitForSeconds(1f);
            currentHealth -= dmg;
            elapsed += 1f;
            if (currentHealth <= 0) { Die(); yield break; }
        }
        if (sprite != null) sprite.color = originalColor;
        burnRoutine = null;
    }

    IEnumerator DamageFlash()
    {
        sprite.color = Color.red; // Âñïûøêà êðàñíûì
        yield return new WaitForSeconds(0.15f);
        sprite.color = (burnRoutine != null) ? new Color(1f, 0.5f, 0f) : originalColor;
        flashRoutine = null;
    }

    // ÌÅÒÎÄ ÄËß ÐÛÂÊÎÂ
    public void BecomeInvincible(float duration)
    {
        StartCoroutine(InvincibilityRoutine(duration));
    }

    IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        if (sprite != null) sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        yield return new WaitForSeconds(duration);
        if (sprite != null) sprite.color = originalColor;
        isInvincible = false;
    }

    void Die()
    {
        var deathScript = GetComponent<PlayerDeath>();
        if (deathScript != null) deathScript.Die();
        else Destroy(gameObject);
    }
}