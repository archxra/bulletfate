using UnityEngine;

public class Bullet2D : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 4f;
    public float lifeTime = 2f;

    [Header("Кого бьем?")]
    public string targetTag = "Enemy"; // Сюда впишем Player для пули врага

    [Header("Стены")]
    public LayerMask obstacleLayer;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Попадание в цель (проверяем по нашему Target Tag)
        if (other.CompareTag(targetTag))
        {
            Health health = other.GetComponent<Health>();
            if (health != null) health.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // 2. Разбивание о стену
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}