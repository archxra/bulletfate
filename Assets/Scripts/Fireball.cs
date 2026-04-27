using UnityEngine;

public class FireballExplosive : MonoBehaviour
{
    public float speed = 15f;
    public float explosionRadius = 2.5f; // Радиус взрыва
    public float explosionDamage = 6f;   // Урон от взрыва
    public LayerMask obstacleLayer;
    public GameObject explosionVisual; // Префаб вспышки взрыва (если есть)

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Если врезались во врага ИЛИ в стену
        if (other.CompareTag("Enemy") || ((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Explode();
        }
    }

    void Explode()
    {
        Debug.Log("БАБАХ!");
        // Создаем вспышку
        if (explosionVisual != null) Instantiate(explosionVisual, transform.position, Quaternion.identity);

        // Ищем всех врагов в радиусе взрыва
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Health h = enemy.GetComponent<Health>();
                if (h != null) h.TakeDamage(explosionDamage);
            }
        }
        Destroy(gameObject); // Уничтожаем сам шар
    }
}