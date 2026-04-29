using UnityEngine;
public class BossWeb : MonoBehaviour
{
    public float damage = 2f;
    public float lifeTime = 3f;
    [Header("Стены")]
    public LayerMask obstacleLayer;
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    // Update НЕТ — движение через rb.linearVelocity из босса
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Health>()?.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}