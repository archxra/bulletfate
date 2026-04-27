using UnityEngine;

public class FireBullet_Logic : MonoBehaviour
{
    public float speed = 20f;
    public float damageOnHit = 1f; // Урон при самом ударе
    public string targetTag = "Enemy";
    public LayerMask obstacleLayer;

    [Header("Настройки Огня")]
    public float burnDamage = 2f; // Сколько хп отнимать в сек
    public float burnDuration = 5f; // Сколько секунд будет гореть

    void Update() => transform.Translate(Vector3.up * speed * Time.deltaTime);

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Если попали в цель
        if (other.CompareTag(targetTag))
        {
            Health h = other.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(damageOnHit); // Обычный урон
                h.ApplyBurn(burnDamage, burnDuration); // ПОДЖОГ
            }
            Destroy(gameObject);
            return;
        }

        // Если попали в стену
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}