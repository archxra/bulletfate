using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;

    [Header("Отображение")]
    public TextMesh hpText;
    private SpriteRenderer sprite;
    private Color originalColor;

    // ЭТО ТЕ САМЫЕ ПЕРЕМЕННЫЕ, КОТОРЫЕ ИЩУТ ДРУГИЕ СКРИПТЫ
    public bool isInvincible = false;

    private Coroutine burnRoutine;
    private Coroutine flashRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) originalColor = sprite.color;
        UpdateText();
    }

    public void TakeDamage(float damageAmount)
    {
        // Если включено бессмертие - урон не проходит
        if (isInvincible) return;

        currentHealth -= damageAmount;
        UpdateText();

        if (sprite != null)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0) Die();
    }

    // МЕТОД ДЛЯ ВКЛЮЧЕНИЯ БЕССМЕРТИЯ (нужен для рывка и ульты 3-го перса)
    public void BecomeInvincible(float duration)
    {
        StartCoroutine(InvincibilityRoutine(duration));
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        if (sprite != null) sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        yield return new WaitForSeconds(duration);
        if (sprite != null) sprite.color = originalColor;
        isInvincible = false;
    }

    // СИСТЕМА ГОРЕНИЯ
    public void ApplyBurn(float tickDamage, float duration)
    {
        if (isInvincible) return;
        if (burnRoutine != null) StopCoroutine(burnRoutine);
        burnRoutine = StartCoroutine(BurnProcess(tickDamage, duration));
    }

    IEnumerator BurnProcess(float dmg, float dur)
    {
        float elapsed = 0;
        if (sprite != null) sprite.color = new Color(1f, 0.5f, 0f);

        while (elapsed < dur)
        {
            yield return new WaitForSeconds(1f);
            currentHealth -= dmg;
            UpdateText();
            elapsed += 1f;

            if (currentHealth <= 0)
            {
                Die();
                yield break;
            }
        }

        if (sprite != null) sprite.color = originalColor;
        burnRoutine = null;
    }

    IEnumerator DamageFlash()
    {
        sprite.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        // Возвращаем правильный цвет (оранжевый если горит, иначе родной)
        if (burnRoutine != null) sprite.color = new Color(1f, 0.5f, 0f);
        else sprite.color = originalColor;

        flashRoutine = null;
    }

    void UpdateText()
    {
        if (hpText != null) hpText.text = currentHealth + " / " + maxHealth;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}